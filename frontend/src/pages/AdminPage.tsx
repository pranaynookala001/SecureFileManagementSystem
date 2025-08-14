import React from 'react';
import {
  Box,
  Typography,
  Paper,
} from '@mui/material';

const AdminPage: React.FC = () => {
  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Admin Dashboard
      </Typography>
      
      <Paper sx={{ p: 3 }}>
        <Typography variant="body1">
          This is a placeholder for admin functionality.
        </Typography>
      </Paper>
    </Box>
  );
};

export default AdminPage;
