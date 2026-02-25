import api from "./api";

export const getLeaves = () => {
  return api.get("/leave");
};