import React from 'react';
import { useParams } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Button,
} from '@mui/material';

const DocumentViewPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Document Details
      </Typography>
      
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Document ID: {id}
        </Typography>
        <Typography variant="body1">
          This is a placeholder for document viewing functionality.
        </Typography>
      </Paper>
    </Box>
  );
};

export default DocumentViewPage;
