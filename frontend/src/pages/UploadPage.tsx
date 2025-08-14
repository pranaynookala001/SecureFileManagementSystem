import React, { useState, useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Alert,
  CircularProgress,
  Grid,
  Card,
  CardContent,
  IconButton,
} from '@mui/material';
import { CloudUpload, Delete, Description, Security } from '@mui/icons-material';
import { useDropzone } from 'react-dropzone';
import { addDocument } from '../store/slices/documentSlice';

interface UploadedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  status: 'uploading' | 'success' | 'error';
  progress: number;
}

const UploadPage: React.FC = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [documentName, setDocumentName] = useState('');
  const [description, setDescription] = useState('');
  const [securityLevel, setSecurityLevel] = useState('confidential');
  const [tags, setTags] = useState<string[]>([]);
  const [newTag, setNewTag] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string>('');

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const newFiles: UploadedFile[] = acceptedFiles.map((file, index) => ({
      id: `file-${Date.now()}-${index}`,
      name: file.name,
      size: file.size,
      type: file.type,
      status: 'uploading' as const,
      progress: 0,
    }));

    setUploadedFiles(prev => [...prev, ...newFiles]);
    simulateUpload(newFiles);
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/pdf': ['.pdf'],
      'application/msword': ['.doc'],
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': ['.docx'],
      'application/vnd.ms-excel': ['.xls'],
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'text/plain': ['.txt'],
      'image/*': ['.png', '.jpg', '.jpeg', '.gif'],
    },
    multiple: true,
  });

  const simulateUpload = (files: UploadedFile[]) => {
    setIsUploading(true);
    setError('');

    files.forEach((file, index) => {
      const interval = setInterval(() => {
        setUploadedFiles(prev => 
          prev.map(f => 
            f.id === file.id 
              ? { ...f, progress: Math.min(f.progress + 10, 100) }
              : f
          )
        );

        if (uploadedFiles.find(f => f.id === file.id)?.progress === 100) {
          clearInterval(interval);
          setUploadedFiles(prev => 
            prev.map(f => 
              f.id === file.id 
                ? { ...f, status: 'success' as const }
                : f
            )
          );
        }
      }, 200);

      // Simulate completion after 2 seconds
      setTimeout(() => {
        clearInterval(interval);
        setUploadedFiles(prev => 
          prev.map(f => 
            f.id === file.id 
              ? { ...f, status: 'success' as const, progress: 100 }
              : f
          )
        );
        setIsUploading(false);
      }, 2000);
    });
  };

  const handleRemoveFile = (fileId: string) => {
    setUploadedFiles(prev => prev.filter(f => f.id !== fileId));
  };

  const handleAddTag = () => {
    if (newTag.trim() && !tags.includes(newTag.trim())) {
      setTags([...tags, newTag.trim()]);
      setNewTag('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setTags(tags.filter(tag => tag !== tagToRemove));
  };

  const handleSubmit = () => {
    if (uploadedFiles.length === 0) {
      setError('Please select at least one file to upload');
      return;
    }

    if (!documentName.trim()) {
      setError('Please enter a document name');
      return;
    }

    // Add each uploaded file as a document to the store
    uploadedFiles.forEach((file) => {
      const newDocument = {
        id: file.id,
        name: documentName || file.name,
        type: file.type.split('/')[1]?.toUpperCase() || 'UNKNOWN',
        size: formatFileSize(file.size),
        uploadedAt: new Date().toISOString().split('T')[0],
        status: 'Encrypted',
        securityLevel: securityLevel.charAt(0).toUpperCase() + securityLevel.slice(1),
        description: description || `Uploaded document: ${file.name}`,
        tags: tags,
        owner: 'Current User',
        lastAccessed: new Date().toISOString().split('T')[0],
        accessCount: 0,
      };

      dispatch(addDocument(newDocument));
    });

    // Show success message
    setError('');
    alert('Documents uploaded successfully!');
    
    // Navigate to documents page
    navigate('/documents');
    
    // Reset form
    setUploadedFiles([]);
    setDocumentName('');
    setDescription('');
    setSecurityLevel('confidential');
    setTags([]);
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Upload Document
      </Typography>
      
      <Grid container spacing={3}>
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Document Information
            </Typography>
            
            <TextField
              fullWidth
              label="Document Name"
              value={documentName}
              onChange={(e) => setDocumentName(e.target.value)}
              margin="normal"
              required
            />
            
            <TextField
              fullWidth
              label="Description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              margin="normal"
              multiline
              rows={3}
            />
            
            <FormControl fullWidth margin="normal">
              <InputLabel>Security Level</InputLabel>
              <Select
                value={securityLevel}
                onChange={(e) => setSecurityLevel(e.target.value)}
                label="Security Level"
              >
                <MenuItem value="public">Public</MenuItem>
                <MenuItem value="internal">Internal</MenuItem>
                <MenuItem value="confidential">Confidential</MenuItem>
                <MenuItem value="secret">Secret</MenuItem>
              </Select>
            </FormControl>
            
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Tags
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, mb: 1 }}>
                <TextField
                  size="small"
                  placeholder="Add tag"
                  value={newTag}
                  onChange={(e) => setNewTag(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && handleAddTag()}
                />
                <Button variant="outlined" onClick={handleAddTag}>
                  Add
                </Button>
              </Box>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {tags.map((tag) => (
                  <Chip
                    key={tag}
                    label={tag}
                    onDelete={() => handleRemoveTag(tag)}
                    color="primary"
                    variant="outlined"
                  />
                ))}
              </Box>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Security Features
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Security color="primary" sx={{ mr: 1 }} />
              <Typography variant="body2">
                End-to-end encryption enabled
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Description color="primary" sx={{ mr: 1 }} />
              <Typography variant="body2">
                Automatic virus scanning
              </Typography>
            </Box>
            <Typography variant="body2" color="text.secondary">
              All uploaded documents are automatically encrypted and scanned for security threats.
            </Typography>
          </Paper>
        </Grid>
      </Grid>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          File Upload
        </Typography>
        
        <Box
          {...getRootProps()}
          sx={{
            border: '2px dashed',
            borderColor: isDragActive ? 'primary.main' : 'grey.300',
            borderRadius: 2,
            p: 4,
            textAlign: 'center',
            cursor: 'pointer',
            backgroundColor: isDragActive ? 'action.hover' : 'background.paper',
            transition: 'all 0.2s',
            '&:hover': {
              borderColor: 'primary.main',
              backgroundColor: 'action.hover',
            },
          }}
        >
          <input {...getInputProps()} />
          <CloudUpload sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
          <Typography variant="h6" gutterBottom>
            {isDragActive ? 'Drop files here' : 'Drag & drop files here'}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            or click to select files
          </Typography>
          <Typography variant="caption" display="block" sx={{ mt: 1 }}>
            Supported formats: PDF, DOC, DOCX, XLS, XLSX, TXT, Images
          </Typography>
        </Box>
      </Paper>

      {uploadedFiles.length > 0 && (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Uploaded Files
          </Typography>
          {uploadedFiles.map((file) => (
            <Card key={file.id} sx={{ mb: 2 }}>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box>
                    <Typography variant="subtitle1">{file.name}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {formatFileSize(file.size)} • {file.type}
                    </Typography>
                    {file.status === 'uploading' && (
                      <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                        <CircularProgress size={16} sx={{ mr: 1 }} />
                        <Typography variant="body2">
                          Uploading... {file.progress}%
                        </Typography>
                      </Box>
                    )}
                    {file.status === 'success' && (
                      <Typography variant="body2" color="success.main" sx={{ mt: 1 }}>
                        ✓ Upload complete
                      </Typography>
                    )}
                  </Box>
                  <IconButton
                    onClick={() => handleRemoveFile(file.id)}
                    color="error"
                    disabled={file.status === 'uploading'}
                  >
                    <Delete />
                  </IconButton>
                </Box>
              </CardContent>
            </Card>
          ))}
        </Paper>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      <Box sx={{ display: 'flex', gap: 2 }}>
        <Button
          variant="contained"
          size="large"
          onClick={handleSubmit}
          disabled={isUploading || uploadedFiles.length === 0}
          startIcon={isUploading ? <CircularProgress size={20} /> : <CloudUpload />}
        >
          {isUploading ? 'Uploading...' : 'Upload Documents'}
        </Button>
        <Button
          variant="outlined"
          size="large"
          onClick={() => {
            setUploadedFiles([]);
            setDocumentName('');
            setDescription('');
            setSecurityLevel('confidential');
            setTags([]);
            setError('');
          }}
        >
          Clear All
        </Button>
      </Box>
    </Box>
  );
};

export default UploadPage;
