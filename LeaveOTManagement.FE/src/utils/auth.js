export function getUserId() {
    const token = localStorage.getItem("token")

    if (!token) return null

    const payload = JSON.parse(atob(token.split(".")[1]))

    return payload[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    ]
}

export function getUserRole() {
    const token = localStorage.getItem("token")

    if (!token) return null

    const payload = JSON.parse(atob(token.split(".")[1]))

    return payload[
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ]
}

export function getUserName() {
    const token = localStorage.getItem("token")

    if (!token) return null

    const payload = JSON.parse(atob(token.split(".")[1]))

    return payload[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
    ]
}