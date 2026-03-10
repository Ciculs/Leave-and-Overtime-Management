import api from "./api"

// ===============================
// GET ALL HOLIDAYS
// ===============================
export const getHolidays = () => {
  return api.get("/holidays")
}

// ===============================
// GET HOLIDAYS BY YEAR (AUTO LOAD)
// ===============================
export const getHolidayByYear = (year) => {
  return api.get(`/holidays/year/${year}`)
}

// ===============================
// IMPORT HOLIDAY EXCEL
// ===============================
export const importHoliday = (file) => {
  const formData = new FormData()
  formData.append("file", file)

  return api.post("/holidays/import", formData, {
    headers: {
      "Content-Type": "multipart/form-data"
    }
  })
}