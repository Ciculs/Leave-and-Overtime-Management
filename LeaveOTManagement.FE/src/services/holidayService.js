import axios from "axios";

const API_URL = "https://localhost:7121/api/Holiday";

export const getHolidays = async () => {
  return await axios.get(API_URL);
};

export const importHoliday = async (file) => {
  const formData = new FormData();
  formData.append("file", file);

  return await axios.post(`${API_URL}/import`, formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });
};