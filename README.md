# Secure Document Management System

A comprehensive, security-focused document management platform demonstrating Microsoft SWE intern skills.

## 🏗️ Architecture

- **Backend**: C#/.NET 8 Web API with secure authentication/authorization
- **Frontend**: React 18 + TypeScript with modern UI/UX
- **Systems**: Rust microservice for high-performance file processing
- **Security**: End-to-end encryption, JWT tokens, role-based access control
- **DevOps**: Docker containers, Kubernetes manifests
- **Networking**: Custom API gateway, secure communication protocols

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Rust 1.70+
- Docker & Docker Compose
- PostgreSQL 15+

### Running the Application

1. **Clone and setup:**
```bash
git clone <repository>
cd secure-document-management
```

2. **Start with Docker Compose:**
```bash
docker-compose up -d
```

3. **Access the application:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Admin Dashboard: http://localhost:3000/admin

## 🔐 Security Features

- **End-to-end encryption** for all documents
- **Multi-factor authentication** (TOTP, SMS, Email)
- **Role-based access control** with fine-grained permissions
- **Audit logging** for compliance and security monitoring
- **Threat detection** and automated incident response
- **Data loss prevention** with content scanning
- **Secure file sharing** with time-limited links

## 🛠️ Technology Stack

### Backend (.NET 8)
- ASP.NET Core Web API
- Entity Framework Core
- JWT Authentication
- SignalR for real-time features
- Serilog for structured logging

### Frontend (React 18)
- TypeScript for type safety
- Material-UI for modern components
- React Query for state management
- React Router for navigation
- WebSocket for real-time updates

### Systems (Rust)
- High-performance file processing
- Encryption/decryption engine
- Document format conversion
- Cross-platform compatibility

### DevOps
- Docker containers
- Kubernetes manifests
- GitHub Actions CI/CD
- Terraform for infrastructure

## 📁 Project Structure

```
secure-document-management/
├── backend/                 # C#/.NET Web API
├── frontend/               # React/TypeScript application
├── rust-service/           # Rust file processing service
├── k8s/                   # Kubernetes manifests
├── docker/                # Docker configurations
├── docs/                  # Documentation
└── scripts/               # Deployment scripts
```

## 🔧 Development

### Backend Development
```bash
cd backend
dotnet restore
dotnet run
```

### Frontend Development
```bash
cd frontend
npm install
npm start
```

### Rust Service Development
```bash
cd rust-service
cargo build
cargo run
```

## 🧪 Testing

```bash
# Backend tests
cd backend && dotnet test

# Frontend tests
cd frontend && npm test

# Rust tests
cd rust-service && cargo test

# Integration tests
docker-compose -f docker-compose.test.yml up
```

## 📊 Monitoring & Logging

- **Application Insights** for .NET monitoring
- **Prometheus + Grafana** for metrics
- **ELK Stack** for log aggregation
- **Security event monitoring** with custom dashboards

## 🔒 Compliance Features

- **GDPR compliance** tools and data handling
- **HIPAA compliance** for healthcare documents
- **SOX compliance** for financial data
- **Automated compliance reporting**
- **Data retention policies**

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

MIT License - see LICENSE file for details

## 🎯 Skills Demonstrated

This project showcases:
- ✅ **Backend/Services**: C#/.NET, API design, multithreaded programming
- ✅ **Frontend**: TypeScript/JavaScript, React, modern UI engineering
- ✅ **Systems**: Rust, OS concepts, networking, security tooling
- ✅ **Security**: Secure coding, authentication/authorization, compliance
- ✅ **DevOps**: Containers, Kubernetes, CI/CD pipelines
