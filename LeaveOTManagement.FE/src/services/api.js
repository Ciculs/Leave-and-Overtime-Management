import axios from "axios"

const api = axios.create({
  baseURL: "https://localhost:7121/api",
  timeout: 10000
})


/* ADD TOKEN TO EVERY REQUEST */

api.interceptors.request.use(

  (config) => {

    const token = localStorage.getItem("token")

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  },

  (error) => {
    return Promise.reject(error)
  }

)


/* HANDLE TOKEN EXPIRED */

api.interceptors.response.use(

  (response) => response,

  (error) => {

    if (error.response && error.response.status === 401) {

      console.warn("Token expired")

    }

    return Promise.reject(error)
  }

)

export default api