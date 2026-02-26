import api from "./api";

export const getLeaves = () => {
  return api.get("/leave");
};

export const getLeaveBalance = (userId) => {
  return api.get(`/leave/balance/${userId}`);
};

export const createLeaveRequest = (payload) => {
  return api.post("/leave/request", payload);
};

export const getLeaveList = (userId) => {
  return api.get(`/leave/list/${userId}`);
};