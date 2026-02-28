<template>
  <div class="login-container">

    <!-- LEFT SIDE -->
    <div class="login-left">
      <div class="overlay">
        <h1>Leave & Overtime Management System</h1>
        <p>Enterprise Workforce Platform</p>
      </div>
    </div>

    <!-- RIGHT SIDE -->
    <div class="login-right">
      <div class="right-content">

        <div class="welcome">
          <h2>Welcome Back 👋</h2>
          <p>Sign in to continue to your dashboard</p>
        </div>

        <div class="card">
          <h3>Sign In</h3>

          <input v-model="username" type="text" placeholder="Username" />
          <input v-model="password" type="password" placeholder="Password" />

          <button class="btn-login" @click="login">
            Log In
          </button>

          <div class="forgot-wrapper">
            <a href="#" class="forgot-link">
              Forgotten password?
            </a>
          </div>

          <hr />

          <button class="btn-register">
            Create New Account
          </button>

          <p v-if="error" class="error">{{ error }}</p>
        </div>

        <div class="stats">
          <div>✔ 500+ Employees</div>
          <div>✔ 1200+ Requests Processed</div>
          <div>✔ Secure Enterprise Platform</div>
        </div>

      </div>
    </div>

  </div>
</template>

<script setup>
import { ref } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()

const username = ref("")
const password = ref("")
const error = ref("")

const login = async () => {
  error.value = ""

  try {
    const res = await api.post("/Auth/login", {
      username: username.value,
      password: password.value,
    })

    const { token, role } = res.data

    localStorage.setItem("token", token)
    localStorage.setItem("role", role)

    if (role === "Admin") {
      router.push("/admin")
    } 
    else if (role === "Manager") {
      router.push("/manager")
    } 
    else if (role === "Employee") {
      router.push("/employee")
    } 
    else {
      router.push("/")
    }

  } catch (err) {
    error.value = "Invalid username or password"
  }
}
</script>

<style scoped>

.login-container {
  display: flex;
  height: 100vh;
  font-family: 'Segoe UI', sans-serif;
}

/* LEFT SIDE */
.login-left {
  flex: 1;
  background-image: url("public/images/kkk.png");
  background-size: cover;
  background-position: center;
  position: relative;
}

.overlay {
  
  color: white;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 40px;
  text-align: center;
  margin-top: -100px;
}

/* RIGHT SIDE */
.login-right {
  flex: 1;
  background: #f4f7fb;
  display: flex;
  justify-content: center;
  align-items: center;
}

.right-content {
  width: 520px;
  display: flex;
  flex-direction: column;
  gap: 25px;
}

/* Welcome text */
.welcome h2 {
  margin: 0;
  font-size: 26px;
  color: #1e3c72;
}

.welcome p {
  margin: 5px 0 0 0;
  color: #666;
  font-size: 14px;
}

/* Card */
.card {
  background: white;
  padding: 40px;
  border-radius: 15px;
  box-shadow: 0 15px 40px rgba(0,0,0,0.08);
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.card h3 {
  margin-bottom: 5px;
}

/* Stats */
.stats {
  font-size: 13px;
  color: #555;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-left: 5px;
}

/* INPUT */
input {
  padding: 13px;
  border-radius: 8px;
  border: 1px solid #ddd;
  font-size: 14px;
}

input:focus {
  outline: none;
  border-color: #18e3f2;
  box-shadow: 0 0 0 2px rgba(24,119,242,0.2);
}

/* LOGIN BUTTON (Facebook blue) */
.btn-login {
  padding: 12px;
  border-radius: 8px;
  border: none;
  background: #34bde7;
  color: white;
  font-weight: bold;
  font-size: 16px;
  cursor: pointer;
  transition: 0.3s;
}

.btn-login:hover {
  background: #35b7ce;
}

/* FORGOT PASSWORD */
.forgot-wrapper {
  text-align: center;
}

.forgot-link {
  color: #1fb3b3;
  font-size: 14px;
  text-decoration: none;
}

.forgot-link:hover {
  text-decoration: underline;
}

/* DIVIDER */
hr {
  border: none;
  border-top: 1px solid #ddd;
}

/* REGISTER BUTTON (Facebook green) */
.btn-register {
  padding: 12px;
  border-radius: 8px;
  border: none;
  background: #36eeee;
  color: white;
  font-weight: bold;
  font-size: 15px;
  cursor: pointer;
  transition: 0.3s;
}

.btn-register:hover {
  background: #29dbcc;
}

.error {
  color: red;
  text-align: center;
  font-size: 14px;
}

</style>