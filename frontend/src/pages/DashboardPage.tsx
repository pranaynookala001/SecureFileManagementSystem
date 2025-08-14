import React from 'react';
import { useSelector } from 'react-redux';
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActions,
  Button,
  LinearProgress,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
} from '@mui/material';
import {
  Description,
  CloudUpload,
  Security,
  TrendingUp,
  Warning,
  CheckCircle,
  Schedule,
  Storage,
  Person,
  Visibility,
  Download,
  Share,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { RootState } from '../store';

const DashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const documents = useSelector((state: RootState) => state.documents.documents);

  // Calculate stats from actual documents
  const stats = {
    totalDocuments: documents.length,
    totalSize: '2.4 GB', // This would be calculated from actual file sizes
    encryptedDocuments: documents.length, // All documents are encrypted
    recentUploads: documents.filter(doc => {
      const uploadDate = new Date(doc.uploadedAt);
      const weekAgo = new Date();
      weekAgo.setDate(weekAgo.getDate() - 7);
      return uploadDate > weekAgo;
    }).length,
    securityAlerts: 2,
    storageUsed: 65,
  };

  const recentDocuments = documents.slice(0, 4).map(doc => ({
    name: doc.name,
    type: doc.type,
    uploaded: getTimeAgo(doc.uploadedAt),
    security: doc.securityLevel,
  }));

  function getTimeAgo(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60));
    
    if (diffInHours < 1) return 'Just now';
    if (diffInHours < 24) return `${diffInHours} hours ago`;
    if (diffInHours < 48) return '1 day ago';
    return `${Math.floor(diffInHours / 24)} days ago`;
  }

  const securityAlerts = [
    { type: 'Suspicious Access', description: 'Multiple failed login attempts detected', severity: 'Medium' },
    { type: 'Document Access', description: 'Unusual document access pattern detected', severity: 'Low' },
  ];

  const quickActions = [
    { title: 'Upload Document', icon: <CloudUpload />, action: () => navigate('/upload'), color: 'primary' },
    { title: 'View Documents', icon: <Description />, action: () => navigate('/documents'), color: 'secondary' },
    { title: 'Security Audit', icon: <Security />, action: () => navigate('/audit'), color: 'warning' },
    { title: 'User Profile', icon: <Person />, action: () => navigate('/profile'), color: 'info' },
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Dashboard
      </Typography>
      
      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" component="div">
                    {stats.totalDocuments}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Total Documents
                  </Typography>
                </Box>
                <Description color="primary" sx={{ fontSize: 40 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" component="div">
                    {stats.totalSize}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Total Storage
                  </Typography>
                </Box>
                <Storage color="secondary" sx={{ fontSize: 40 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" component="div">
                    {stats.encryptedDocuments}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Encrypted
                  </Typography>
                </Box>
                <Security color="success" sx={{ fontSize: 40 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" component="div">
                    {stats.recentUploads}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Recent Uploads
                  </Typography>
                </Box>
                <TrendingUp color="info" sx={{ fontSize: 40 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        {/* Quick Actions */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Quick Actions
              </Typography>
              <Grid container spacing={2}>
                {quickActions.map((action, index) => (
                  <Grid item xs={6} key={index}>
                    <Button
                      variant="outlined"
                      fullWidth
                      startIcon={action.icon}
                      onClick={action.action}
                      sx={{ height: 60, flexDirection: 'column' }}
                    >
                      {action.title}
                    </Button>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        {/* Storage Usage */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Storage Usage
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Box display="flex" justifyContent="space-between" mb={1}>
                  <Typography variant="body2">Used Space</Typography>
                  <Typography variant="body2">{stats.storageUsed}%</Typography>
                </Box>
                <LinearProgress 
                  variant="determinate" 
                  value={stats.storageUsed} 
                  sx={{ height: 8, borderRadius: 4 }}
                />
              </Box>
              <Typography variant="body2" color="text.secondary">
                {stats.totalSize} of 4 GB used
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Security Alerts */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Security Alerts
              </Typography>
              {securityAlerts.length > 0 ? (
                <List dense>
                  {securityAlerts.map((alert, index) => (
                    <React.Fragment key={index}>
                      <ListItem>
                        <ListItemIcon>
                          <Warning color="warning" />
                        </ListItemIcon>
                        <ListItemText
                          primary={alert.type}
                          secondary={alert.description}
                        />
                        <Chip 
                          label={alert.severity} 
                          size="small" 
                          color={alert.severity === 'High' ? 'error' : 'warning'}
                        />
                      </ListItem>
                      {index < securityAlerts.length - 1 && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              ) : (
                <Box display="flex" alignItems="center" sx={{ color: 'success.main' }}>
                  <CheckCircle sx={{ mr: 1 }} />
                  <Typography variant="body2">No security alerts</Typography>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Documents */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Documents
              </Typography>
              <List>
                {recentDocuments.map((doc, index) => (
                  <React.Fragment key={index}>
                    <ListItem>
                      <ListItemIcon>
                        <Description color="primary" />
                      </ListItemIcon>
                      <ListItemText
                        primary={doc.name}
                        secondary={`${doc.type} • Uploaded ${doc.uploaded}`}
                      />
                      <Chip 
                        label={doc.security} 
                        size="small" 
                        color={doc.security === 'Secret' ? 'error' : 'primary'}
                        sx={{ mr: 1 }}
                      />
                      <Button size="small" startIcon={<Visibility />}>
                        View
                      </Button>
                      <Button size="small" startIcon={<Download />}>
                        Download
                      </Button>
                    </ListItem>
                    {index < recentDocuments.length - 1 && <Divider />}
                  </React.Fragment>
                ))}
              </List>
              <Box sx={{ mt: 2, textAlign: 'center' }}>
                <Button variant="outlined" onClick={() => navigate('/documents')}>
                  View All Documents
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default DashboardPage;
