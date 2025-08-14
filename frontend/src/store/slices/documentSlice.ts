import { createSlice, PayloadAction } from '@reduxjs/toolkit';

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

interface DocumentState {
  documents: Document[];
  isLoading: boolean;
  error: string | null;
}

const initialState: DocumentState = {
  documents: [
    {
      id: '1',
      name: 'Q4 Financial Report 2024',
      type: 'PDF',
      size: '2.5 MB',
      uploadedAt: '2024-01-15',
      status: 'Encrypted',
      securityLevel: 'Confidential',
      description: 'Quarterly financial report for Q4 2024',
      tags: ['finance', 'quarterly', 'reports'],
      owner: 'John Doe',
      lastAccessed: '2024-01-20',
      accessCount: 15,
    },
    {
      id: '2',
      name: 'Employee Handbook 2024',
      type: 'DOCX',
      size: '1.8 MB',
      uploadedAt: '2024-01-14',
      status: 'Encrypted',
      securityLevel: 'Internal',
      description: 'Updated employee handbook for 2024',
      tags: ['hr', 'policy', 'handbook'],
      owner: 'Jane Smith',
      lastAccessed: '2024-01-19',
      accessCount: 42,
    },
    {
      id: '3',
      name: 'Security Audit Report',
      type: 'PDF',
      size: '3.2 MB',
      uploadedAt: '2024-01-13',
      status: 'Encrypted',
      securityLevel: 'Secret',
      description: 'Annual security audit findings and recommendations',
      tags: ['security', 'audit', 'annual'],
      owner: 'Mike Johnson',
      lastAccessed: '2024-01-18',
      accessCount: 8,
    },
    {
      id: '4',
      name: 'Marketing Strategy 2024',
      type: 'PPTX',
      size: '4.1 MB',
      uploadedAt: '2024-01-12',
      status: 'Encrypted',
      securityLevel: 'Confidential',
      description: 'Marketing strategy and campaign plans for 2024',
      tags: ['marketing', 'strategy', 'campaigns'],
      owner: 'Sarah Wilson',
      lastAccessed: '2024-01-17',
      accessCount: 23,
    },
  ],
  isLoading: false,
  error: null,
};

const documentSlice = createSlice({
  name: 'documents',
  initialState,
  reducers: {
    fetchDocumentsStart: (state) => {
      state.isLoading = true;
      state.error = null;
    },
    fetchDocumentsSuccess: (state, action: PayloadAction<Document[]>) => {
      state.isLoading = false;
      state.documents = action.payload;
    },
    fetchDocumentsFailure: (state, action: PayloadAction<string>) => {
      state.isLoading = false;
      state.error = action.payload;
    },
    addDocument: (state, action: PayloadAction<Document>) => {
      state.documents.unshift(action.payload); // Add to beginning of array
    },
    removeDocument: (state, action: PayloadAction<string>) => {
      state.documents = state.documents.filter(doc => doc.id !== action.payload);
    },
    updateDocument: (state, action: PayloadAction<Document>) => {
      const index = state.documents.findIndex(doc => doc.id === action.payload.id);
      if (index !== -1) {
        state.documents[index] = action.payload;
      }
    },
  },
});

export const { 
  fetchDocumentsStart, 
  fetchDocumentsSuccess, 
  fetchDocumentsFailure, 
  addDocument, 
  removeDocument,
  updateDocument
} = documentSlice.actions;

export default documentSlice.reducer;
