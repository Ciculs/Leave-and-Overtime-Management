import api from "./api"

export const getHolidays = () => {
  return api.get("/holidays")
}

export const getHolidayByYear = (year) => {
  return api.get(`/holidays/year/${year}`)
}

export const syncHolidayByYear = (year) => {
  return api.post(`/holidays/sync/${year}`)
}

export const createHoliday = (payload) => {
  return api.post("/holidays", payload)
}

export const updateHoliday = (id, payload) => {
  return api.put(`/holidays/${id}`, payload)
}

export const deleteHoliday = (id) => {
  return api.delete(`/holidays/${id}`)
}

export const importHoliday = (file) => {
  const formData = new FormData()
  formData.append("file", file)

  return api.post("/holidays/import", formData, {
    headers: {
      "Content-Type": "multipart/form-data"
    }
  })
}