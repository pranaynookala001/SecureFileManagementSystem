import React, { useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
  Box,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TextField,
  InputAdornment,
  IconButton,
  Chip,
  Menu,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  Grid,
  Card,
  CardContent,
  CardActions,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
} from '@mui/material';
import {
  Search,
  FilterList,
  MoreVert,
  Download,
  Share,
  Delete,
  Edit,
  Visibility,
  Security,
  Description,
  CalendarToday,
  Storage,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { removeDocument } from '../store/slices/documentSlice';
import { RootState } from '../store';

interface Document {
  id: string;
  name: string;
  type: string;
  size: string;
  uploadedAt: string;
  status: string;
  securityLevel: string;
  description: string;
  tags: string[];
  owner: string;
  lastAccessed: string;
  accessCount: number;
}

const DocumentsPage: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const documents = useSelector((state: RootState) => state.documents.documents);
  
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState('all');
  const [filterSecurity, setFilterSecurity] = useState('all');
  const [viewMode, setViewMode] = useState<'table' | 'grid'>('table');
  const [selectedDocument, setSelectedDocument] = useState<Document | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const filteredDocuments = documents.filter(doc => {
    const matchesSearch = doc.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         doc.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         doc.tags.some(tag => tag.toLowerCase().includes(searchTerm.toLowerCase()));
    const matchesType = filterType === 'all' || doc.type.toLowerCase() === filterType.toLowerCase();
    const matchesSecurity = filterSecurity === 'all' || doc.securityLevel.toLowerCase() === filterSecurity.toLowerCase();
    
    return matchesSearch && matchesType && matchesSecurity;
  });

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>, document: Document) => {
    setAnchorEl(event.currentTarget);
    setSelectedDocument(document);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
    setSelectedDocument(null);
  };

  const handleDelete = () => {
    if (selectedDocument) {
      dispatch(removeDocument(selectedDocument.id));
      alert(`Document "${selectedDocument.name}" deleted successfully`);
    }
    setDeleteDialogOpen(false);
    handleMenuClose();
  };

  const getSecurityColor = (level: string) => {
    switch (level.toLowerCase()) {
      case 'public': return 'success';
      case 'internal': return 'info';
      case 'confidential': return 'warning';
      case 'secret': return 'error';
      default: return 'default';
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Documents ({filteredDocuments.length})
        </Typography>
        <Button
          variant="contained"
          onClick={() => navigate('/upload')}
        >
          Upload Document
        </Button>
      </Box>

      {/* Search and Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={4}>
            <TextField
              fullWidth
              placeholder="Search documents..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Search />
                  </InputAdornment>
                ),
              }}
            />
          </Grid>
          <Grid item xs={12} md={2}>
            <FormControl fullWidth>
              <InputLabel>Type</InputLabel>
              <Select
                value={filterType}
                onChange={(e) => setFilterType(e.target.value)}
                label="Type"
              >
                <MenuItem value="all">All Types</MenuItem>
                <MenuItem value="pdf">PDF</MenuItem>
                <MenuItem value="docx">DOCX</MenuItem>
                <MenuItem value="pptx">PPTX</MenuItem>
                <MenuItem value="xlsx">XLSX</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={2}>
            <FormControl fullWidth>
              <InputLabel>Security</InputLabel>
              <Select
                value={filterSecurity}
                onChange={(e) => setFilterSecurity(e.target.value)}
                label="Security"
              >
                <MenuItem value="all">All Levels</MenuItem>
                <MenuItem value="public">Public</MenuItem>
                <MenuItem value="internal">Internal</MenuItem>
                <MenuItem value="confidential">Confidential</MenuItem>
                <MenuItem value="secret">Secret</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={2}>
            <Button
              variant="outlined"
              startIcon={<FilterList />}
              onClick={() => setViewMode(viewMode === 'table' ? 'grid' : 'table')}
            >
              {viewMode === 'table' ? 'Grid View' : 'Table View'}
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {/* Documents Display */}
      {viewMode === 'table' ? (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Size</TableCell>
                <TableCell>Security Level</TableCell>
                <TableCell>Uploaded</TableCell>
                <TableCell>Last Accessed</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredDocuments.map((doc) => (
                <TableRow key={doc.id} hover>
                  <TableCell>
                    <Box>
                      <Typography variant="subtitle2">{doc.name}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {doc.description}
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>{doc.type}</TableCell>
                  <TableCell>{doc.size}</TableCell>
                  <TableCell>
                    <Chip
                      label={doc.securityLevel}
                      color={getSecurityColor(doc.securityLevel) as any}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>{formatDate(doc.uploadedAt)}</TableCell>
                  <TableCell>{formatDate(doc.lastAccessed)}</TableCell>
                  <TableCell>
                    <Chip
                      label={doc.status}
                      color="success"
                      size="small"
                      icon={<Security />}
                    />
                  </TableCell>
                  <TableCell>
                    <IconButton onClick={(e) => handleMenuClick(e, doc)}>
                      <MoreVert />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Grid container spacing={3}>
          {filteredDocuments.map((doc) => (
            <Grid item xs={12} sm={6} md={4} lg={3} key={doc.id}>
              <Card>
                <CardContent>
                  <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
                    <Description color="primary" />
                    <IconButton size="small" onClick={(e) => handleMenuClick(e, doc)}>
                      <MoreVert />
                    </IconButton>
                  </Box>
                  <Typography variant="h6" gutterBottom noWrap>
                    {doc.name}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    {doc.description}
                  </Typography>
                  <Box display="flex" flexWrap="wrap" gap={0.5} mb={2}>
                    {doc.tags.slice(0, 3).map((tag) => (
                      <Chip key={tag} label={tag} size="small" variant="outlined" />
                    ))}
                    {doc.tags.length > 3 && (
                      <Chip label={`+${doc.tags.length - 3}`} size="small" />
                    )}
                  </Box>
                  <Box display="flex" justifyContent="space-between" alignItems="center">
                    <Chip
                      label={doc.securityLevel}
                      color={getSecurityColor(doc.securityLevel) as any}
                      size="small"
                    />
                    <Typography variant="caption" color="text.secondary">
                      {doc.size}
                    </Typography>
                  </Box>
                </CardContent>
                <CardActions>
                  <Button
                    size="small"
                    startIcon={<Visibility />}
                    onClick={() => navigate(`/documents/${doc.id}`)}
                  >
                    View
                  </Button>
                  <Button size="small" startIcon={<Download />}>
                    Download
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Action Menu */}
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => { navigate(`/documents/${selectedDocument?.id}`); handleMenuClose(); }}>
          <Visibility sx={{ mr: 1 }} /> View
        </MenuItem>
        <MenuItem onClick={handleMenuClose}>
          <Download sx={{ mr: 1 }} /> Download
        </MenuItem>
        <MenuItem onClick={handleMenuClose}>
          <Share sx={{ mr: 1 }} /> Share
        </MenuItem>
        <MenuItem onClick={handleMenuClose}>
          <Edit sx={{ mr: 1 }} /> Edit
        </MenuItem>
        <MenuItem onClick={() => setDeleteDialogOpen(true)} sx={{ color: 'error.main' }}>
          <Delete sx={{ mr: 1 }} /> Delete
        </MenuItem>
      </Menu>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Document</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete "{selectedDocument?.name}"? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDelete} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default DocumentsPage;
