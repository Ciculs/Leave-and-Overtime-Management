import api from "./api"

export const getMyOT = () => {
    return api.get("/ot")
}
export const getTeamOT = (year, month) => {
    return api.get(`/ot/team-calendar?year=${year}&month=${month}`)
}