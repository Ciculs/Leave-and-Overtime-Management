import api from "./api"

export const getMyProfile = () => {
    return api.get("/Profile/me")
}

export const updateMyProfile = (payload) => {
    return api.put("/Profile/me", payload)
}

export const sendChangePasswordOtp = (payload) => {
    return api.post("/Profile/send-change-password-otp", payload)
}

export const confirmChangePassword = (payload) => {
    return api.post("/Profile/confirm-change-password", payload)
}