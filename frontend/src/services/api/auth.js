import { apiClient } from './axios';

// =============================================
// Authentication API Service
// =============================================

export const authApi = {
  /**
   * Login user with email and password
   */
  login: async (credentials) => {
    const response = await apiClient.post('/auth/login', credentials);
    return response.data.data; // Extract data from ApiResponse wrapper
  },

  /**
   * Register new user
   */
  register: async (credentials) => {
    const response = await apiClient.post('/auth/register', credentials);
    return response.data.data; // Extract data from ApiResponse wrapper
  },

  /**
   * Logout user
   */
  logout: async () => {
    // Backend doesn't have logout endpoint, just clear local data
    return Promise.resolve();
  },

  /**
   * Get current user profile
   */
  getCurrentUser: async () => {
    const response = await apiClient.get('/auth/me');
    return response.data.data; // Extract data from ApiResponse wrapper
  },

  /**
   * Refresh access token
   */
  refreshToken: async () => {
    const response = await apiClient.post('/auth/refresh');
    return response.data.data; // Extract data from ApiResponse wrapper
  },

  /**
   * Request password reset
   */
  forgotPassword: async (email) => {
    const response = await apiClient.post('/auth/forgot-password', { email });
    return response.data;
  },

  /**
   * Reset password with token
   */
  resetPassword: async (token, password) => {
    const response = await apiClient.post('/auth/reset-password', { token, password });
    return response.data;
  },

  /**
   * Update user profile
   */
  updateProfile: async (data) => {
    const response = await apiClient.patch('/auth/profile', data);
    return response.data.data; // Extract data from ApiResponse wrapper
  },

  /**
   * Change password
   */
  changePassword: async (currentPassword, newPassword) => {
    const response = await apiClient.post('/auth/change-password', { currentPassword, newPassword });
    return response.data;
  },

  /**
   * Upload user avatar
   */
  uploadAvatar: async (formData) => {
    const response = await apiClient.post('/auth/upload-avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return response.data.data;
  },

  /**
   * Delete current user account
   */
  deleteAccount: async () => {
    const response = await apiClient.delete('/auth/me');
    return response.data;
  },
};

export default authApi;
