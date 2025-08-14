use axum::{
    routing::{get, post},
    Router,
    http::StatusCode,
    Json,
    extract::State,
};
use serde::{Deserialize, Serialize};
use std::sync::Arc;
use tower_http::cors::{CorsLayer, Any};
use tracing::{info, error};

mod encryption;
mod file_processor;
mod security;
mod config;

use encryption::EncryptionService;
use file_processor::FileProcessor;
use security::SecurityScanner;
use config::AppConfig;

#[derive(Clone)]
struct AppState {
    encryption_service: Arc<EncryptionService>,
    file_processor: Arc<FileProcessor>,
    security_scanner: Arc<SecurityScanner>,
    config: Arc<AppConfig>,
}

#[derive(Deserialize)]
struct ProcessFileRequest {
    file_data: String, // Base64 encoded
    file_name: String,
    content_type: String,
    operation: String, // "encrypt", "decrypt", "validate", "scan"
}

#[derive(Serialize)]
struct ProcessFileResponse {
    success: bool,
    data: Option<String>, // Base64 encoded result
    checksum: Option<String>,
    metadata: Option<serde_json::Value>,
    error: Option<String>,
}

#[derive(Serialize)]
struct HealthResponse {
    status: String,
    timestamp: chrono::DateTime<chrono::Utc>,
    version: String,
}

async fn health_check() -> Json<HealthResponse> {
    Json(HealthResponse {
        status: "healthy".to_string(),
        timestamp: chrono::Utc::now(),
        version: env!("CARGO_PKG_VERSION").to_string(),
    })
}

async fn process_file(
    State(state): State<AppState>,
    Json(request): Json<ProcessFileRequest>,
) -> Result<Json<ProcessFileResponse>, StatusCode> {
    let result = match request.operation.as_str() {
        "encrypt" => {
            match state.encryption_service.encrypt_file(&request.file_data, &request.file_name).await {
                Ok((encrypted_data, checksum)) => ProcessFileResponse {
                    success: true,
                    data: Some(encrypted_data),
                    checksum: Some(checksum),
                    metadata: None,
                    error: None,
                },
                Err(e) => {
                    error!("Encryption failed: {}", e);
                    ProcessFileResponse {
                        success: false,
                        data: None,
                        checksum: None,
                        metadata: None,
                        error: Some(e.to_string()),
                    }
                }
            }
        }
        "decrypt" => {
            match state.encryption_service.decrypt_file(&request.file_data, &request.file_name).await {
                Ok((decrypted_data, checksum)) => ProcessFileResponse {
                    success: true,
                    data: Some(decrypted_data),
                    checksum: Some(checksum),
                    metadata: None,
                    error: None,
                },
                Err(e) => {
                    error!("Decryption failed: {}", e);
                    ProcessFileResponse {
                        success: false,
                        data: None,
                        checksum: None,
                        metadata: None,
                        error: Some(e.to_string()),
                    }
                }
            }
        }
        "validate" => {
            match state.file_processor.validate_file(&request.file_data, &request.file_name, &request.content_type).await {
                Ok(metadata) => ProcessFileResponse {
                    success: true,
                    data: None,
                    checksum: None,
                    metadata: Some(metadata),
                    error: None,
                },
                Err(e) => {
                    error!("Validation failed: {}", e);
                    ProcessFileResponse {
                        success: false,
                        data: None,
                        checksum: None,
                        metadata: None,
                        error: Some(e.to_string()),
                    }
                }
            }
        }
        "scan" => {
            match state.security_scanner.scan_file(&request.file_data, &request.file_name).await {
                Ok(scan_result) => ProcessFileResponse {
                    success: true,
                    data: None,
                    checksum: None,
                    metadata: Some(serde_json::to_value(scan_result).unwrap()),
                    error: None,
                },
                Err(e) => {
                    error!("Security scan failed: {}", e);
                    ProcessFileResponse {
                        success: false,
                        data: None,
                        checksum: None,
                        metadata: None,
                        error: Some(e.to_string()),
                    }
                }
            }
        }
        _ => {
            ProcessFileResponse {
                success: false,
                data: None,
                checksum: None,
                metadata: None,
                error: Some("Invalid operation".to_string()),
            }
        }
    };

    Ok(Json(result))
}

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    // Load environment variables
    dotenv::dotenv().ok();

    // Initialize tracing
    tracing_subscriber::fmt::init();

    info!("Starting Secure Document Processor Service...");

    // Load configuration
    let config = Arc::new(AppConfig::load()?);
    info!("Configuration loaded successfully");

    // Initialize services
    let encryption_service = Arc::new(EncryptionService::new(&config)?);
    let file_processor = Arc::new(FileProcessor::new(&config)?);
    let security_scanner = Arc::new(SecurityScanner::new(&config)?);

    let state = AppState {
        encryption_service,
        file_processor,
        security_scanner,
        config,
    };

    // Configure CORS
    let cors = CorsLayer::new()
        .allow_origin(Any)
        .allow_methods(Any)
        .allow_headers(Any);

    // Create router
    let app = Router::new()
        .route("/health", get(health_check))
        .route("/process", post(process_file))
        .layer(cors)
        .with_state(state);

    // Start server
    let addr = format!("0.0.0.0:{}", std::env::var("API_PORT").unwrap_or_else(|_| "8080".to_string()));
    info!("Server starting on {}", addr);

    axum::Server::bind(&addr.parse()?)
        .serve(app.into_make_service())
        .await?;

    Ok(())
}
