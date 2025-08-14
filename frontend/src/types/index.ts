// User types
export interface User {
  id: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  role: string;
  isActive: boolean;
  emailVerified: boolean;
  twoFactorEnabled: boolean;
  lastLoginAt?: string;
}

// Authentication types
export interface LoginRequest {
  username: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

// Document types
export interface Document {
  id: string;
  name: string;
  description?: string;
  fileName: string;
  fileExtension: string;
  fileSize: number;
  contentType: string;
  version: number;
  isLatestVersion: boolean;
  status: string;
  securityLevel: string;
  isEncrypted: boolean;
  expiresAt?: string;
  lastAccessedAt?: string;
  accessCount: number;
  ownerId: string;
  folderId?: string;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
  tags?: string[];
  permissions?: DocumentPermission[];
}

export interface DocumentPermission {
  id: string;
  documentId: string;
  userId: string;
  canView: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canShare: boolean;
  canDownload: boolean;
  canComment: boolean;
  isActive: boolean;
  expiresAt?: string;
  grantedBy?: string;
  grantedAt: string;
  notes?: string;
}

export interface UploadDocumentRequest {
  file: File;
  name: string;
  description?: string;
  securityLevel: string;
  tags?: string[];
  folderId?: string;
}

export interface UpdateDocumentRequest {
  name?: string;
  description?: string;
  securityLevel?: string;
  tags?: string[];
  folderId?: string;
}

// Folder types
export interface Folder {
  id: string;
  name: string;
  description?: string;
  parentId?: string;
  ownerId: string;
  status: string;
  securityLevel: string;
  documentCount: number;
  totalSize: number;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
  subfolders?: Folder[];
  documents?: Document[];
}

// Audit types
export interface AuditLog {
  id: string;
  timestamp: string;
  action: string;
  entityType: string;
  entityId?: string;
  userId: string;
  username?: string;
  userEmail?: string;
  userRole?: string;
  ipAddress?: string;
  userAgent?: string;
  sessionId?: string;
  severity: string;
  description?: string;
  details?: string;
  resource?: string;
  status: string;
  errorMessage?: string;
  isSecurityEvent: boolean;
  requiresReview: boolean;
  reviewedAt?: string;
  reviewedBy?: string;
  reviewNotes?: string;
}

export interface AuditSummary {
  fromDate: string;
  toDate: string;
  totalEvents: number;
  securityEvents: number;
  errorEvents: number;
  criticalEvents: number;
  eventsRequiringReview: number;
  topActions: ActionSummary[];
  topEntityTypes: EntityTypeSummary[];
  topUsers: UserSummary[];
}

export interface ActionSummary {
  action: string;
  count: number;
}

export interface EntityTypeSummary {
  entityType: string;
  count: number;
}

export interface UserSummary {
  userId: string;
  count: number;
}

// Security types
export interface SecurityEvent {
  id: string;
  timestamp: string;
  eventType: string;
  severity: string;
  source: string;
  userId?: string;
  username?: string;
  ipAddress?: string;
  userAgent?: string;
  description?: string;
  details?: string;
  resource?: string;
  sessionId?: string;
  isResolved: boolean;
  resolvedAt?: string;
  resolvedBy?: string;
  resolutionNotes?: string;
  requiresImmediateAction: boolean;
  actionTakenAt?: string;
  actionTakenBy?: string;
  actionTaken?: string;
  threatLevel?: string;
  category?: string;
}

// API Response types
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Form types
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
}

// Notification types
export interface Notification {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  message: string;
  timestamp: string;
  read: boolean;
}

// Statistics types
export interface DocumentStatistics {
  totalDocuments: number;
  totalSize: number;
  documentsThisMonth: number;
  documentsThisWeek: number;
  documentsToday: number;
  topFileTypes: FileTypeStats[];
  recentActivity: DocumentActivity[];
}

export interface FileTypeStats {
  extension: string;
  count: number;
  totalSize: number;
}

export interface DocumentActivity {
  documentId: string;
  documentName: string;
  action: string;
  timestamp: string;
  userId: string;
  username: string;
}

export interface UserStatistics {
  totalUsers: number;
  activeUsers: number;
  usersThisMonth: number;
  usersThisWeek: number;
  usersToday: number;
  topRoles: RoleStats[];
  recentActivity: UserActivity[];
}

export interface RoleStats {
  role: string;
  count: number;
}

export interface UserActivity {
  userId: string;
  username: string;
  action: string;
  timestamp: string;
  ipAddress?: string;
}
