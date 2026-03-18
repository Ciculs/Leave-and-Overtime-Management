import api from "./api"

// GET MY LEAVE REQUESTS
export const getLeaveList = () => {
  return api.get("/leave/my")
}

// GET MY LEAVE BALANCE
export const getLeaveBalance = () => {
  return api.get("/leave/balances")
}

// CREATE LEAVE REQUEST
export const createLeaveRequest = (payload) => {
  return api.post("/leave", payload)
}

// TEAM CALENDAR
export const getTeamCalendar = (year, month) => {
  return api.get(`/leave/team-calendar?year=${year}&month=${month}`)
}