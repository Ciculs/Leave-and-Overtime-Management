import api from "./api"

export const getMyProfile = () => {
    return api.get("/users/me")
}

export const updateMyProfile = (payload) => {
    return api.put("/users/me", payload)
}