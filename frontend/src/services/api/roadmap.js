import { apiClient } from './axios';

export const roadmapApi = {
    // Get dynamically recommended roadmap based on ML quiz results
    getMyInterestRoadmap: () => apiClient.get('/roadmap/my-interest'),

    // Mark a specific roadmap step as completed
    markStepComplete: (stepId) => apiClient.post(`/roadmap/mark-done/${stepId}`)
};
