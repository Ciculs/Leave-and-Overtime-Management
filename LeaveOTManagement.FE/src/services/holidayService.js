import api from "./api";

// GET holidays
export const getHolidays = () => {
  return api.get("/holidays");
};

// IMPORT holidays
export const importHoliday = (file) => {
  const formData = new FormData();
  formData.append("file", file);

  return api.post("/holidays/import", formData);
};