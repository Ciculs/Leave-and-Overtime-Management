import api from "./api"

export const getMyOT = () => {
    return api.get("/ot")
}