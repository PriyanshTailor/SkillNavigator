import { apiClient } from './axios';

// =============================================
// Mentors API Service
// =============================================

export const mentorsApi = {
  /**
   * Get all mentors
   */
  getAll: async (filters = {}) => {
    const response = await apiClient.get('/mentors', { params: filters });
    return response.data;
  },

  /**
   * Apply as mentor
   */
  applyAsMentor: async (mentorData) => {
    const response = await apiClient.post('/mentors/apply', mentorData);
    return response.data;
  },

  /**
   * Get mentor by ID
   */
  getById: async (id) => {
    const response = await apiClient.get(`/mentors/${id}`);
    return response.data;
  },

  /**
   * Send a mentor request
   */
  requestMentor: async (mentorId, message) => {
    const response = await apiClient.post('/mentors/request', {
      mentorId,
      message,
    });
    return response.data;
  },

  /**
   * Get mentor requests (for students)
   */
  getMyRequests: async () => {
    const response = await apiClient.get('/mentors/my-requests');
    return response.data;
  },

  /**
   * Get received requests (for mentors)
   */
  getReceivedRequests: async () => {
    const response = await apiClient.get('/mentors/received-requests');
    return response.data;
  },

  /**
   * Accept a mentor request
   */
  acceptRequest: async (requestId) => {
    const response = await apiClient.post(`/mentors/requests/${requestId}/accept`);
    return response.data;
  },

  /**
   * Decline a mentor request
   */
  declineRequest: async (requestId) => {
    const response = await apiClient.post(`/mentors/requests/${requestId}/decline`);
    return response.data;
  },

  /**
   * Search mentors by expertise
   */
  searchByExpertise: async (expertise) => {
    const response = await apiClient.get('/mentors/search', {
      params: { expertise },
    });
    return response.data;
  },
};

export default mentorsApi;
