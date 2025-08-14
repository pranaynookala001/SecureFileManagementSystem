import { useState, useEffect, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { useNavigate } from 'react-router-dom';
import { User, LoginRequest, RegisterRequest, AuthResponse } from '../types';
import authService from '../services/authService';

interface UseAuthReturn {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  register: (userData: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  error: string | null;
}

export const useAuth = (): UseAuthReturn => {
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // Check if user is authenticated on mount
  const isAuthenticated = authService.isAuthenticated();

  // Get current user data
  const {
    data: user,
    isLoading,
    error: userError,
  } = useQuery<User>('currentUser', authService.getCurrentUser, {
    enabled: isAuthenticated,
    retry: false,
    onError: () => {
      // If getting user fails, clear auth data
      authService.clearAuthData();
    },
  });

  // Login mutation
  const loginMutation = useMutation<AuthResponse, Error, LoginRequest>(
    authService.login,
    {
      onSuccess: (data) => {
        authService.storeAuthData(data);
        queryClient.setQueryData('currentUser', data.user);
        setError(null);
        navigate('/dashboard');
      },
      onError: (error) => {
        setError(error.message || 'Login failed');
      },
    }
  );

  // Register mutation
  const registerMutation = useMutation<AuthResponse, Error, RegisterRequest>(
    authService.register,
    {
      onSuccess: (data) => {
        authService.storeAuthData(data);
        queryClient.setQueryData('currentUser', data.user);
        setError(null);
        navigate('/dashboard');
      },
      onError: (error) => {
        setError(error.message || 'Registration failed');
      },
    }
  );

  // Logout mutation
  const logoutMutation = useMutation<void, Error, void>(
    authService.logout,
    {
      onSuccess: () => {
        authService.clearAuthData();
        queryClient.clear();
        setError(null);
        navigate('/login');
      },
      onError: (error) => {
        setError(error.message || 'Logout failed');
        // Still clear local data even if server logout fails
        authService.clearAuthData();
        queryClient.clear();
        navigate('/login');
      },
    }
  );

  // Login function
  const login = useCallback(async (credentials: LoginRequest) => {
    setError(null);
    await loginMutation.mutateAsync(credentials);
  }, [loginMutation]);

  // Register function
  const register = useCallback(async (userData: RegisterRequest) => {
    setError(null);
    await registerMutation.mutateAsync(userData);
  }, [registerMutation]);

  // Logout function
  const logout = useCallback(async () => {
    setError(null);
    await logoutMutation.mutateAsync();
  }, [logoutMutation]);

  // Clear error after a delay
  useEffect(() => {
    if (error) {
      const timer = setTimeout(() => {
        setError(null);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [error]);

  return {
    user: user || null,
    isAuthenticated: isAuthenticated && !!user,
    isLoading: isLoading || loginMutation.isLoading || registerMutation.isLoading || logoutMutation.isLoading,
    login,
    register,
    logout,
    error,
  };
};
