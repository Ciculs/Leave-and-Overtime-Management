<template>
  <div class="login-container">

    <div class="login-left">
      <div class="overlay">
        <h1>Leave & Overtime Management System</h1>
        <p>Enterprise Workforce Platform</p>
      </div>
    </div>

    <div class="login-right">
      <div class="card">
        <h2>Sign In</h2>

        <input 
  v-model="username"
  type="text"
  placeholder="Username"
/>

        <input 
          v-model="password"
          type="password"
          placeholder="Password"
        />

        <button @click="login">
          Login
        </button>

        <p v-if="error" class="error">{{ error }}</p>
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

    localStorage.setItem("token", res.data.token)
    localStorage.setItem("role", res.data.role)

    if (res.data.role === "Employee")
      router.push("/leave-request")
    else if (res.data.role === "Manager")
      router.push("/manager")
    else if (res.data.role === "HR")
      router.push("/hr")

  } catch (err) {
    error.value = "Invalid username or password"
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  height: 100vh;
}

/* LEFT SIDE - IMAGE */
.login-left {
  flex: 1;
  background-image: url("/images/company-banner.jpg");
  background-size: cover;
  background-position: center;
  position: relative;
}

.overlay {
  background: rgba(0,0,0,0.6);
  color: white;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}

/* RIGHT SIDE - FORM */
.login-right {
  flex: 1;
  background: #f4f7fb;
  display: flex;
  justify-content: center;
  align-items: center;
}

.card {
  background: white;
  padding: 40px;
  width: 350px;
  border-radius: 15px;
  box-shadow: 0 15px 40px rgba(0,0,0,0.1);
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.card h2 {
  text-align: center;
  margin-bottom: 10px;
}

input {
  padding: 12px;
  border-radius: 8px;
  border: 1px solid #ccc;
}

button {
  padding: 12px;
  border-radius: 8px;
  border: none;
  background: #1e3c72;
  color: white;
  font-weight: bold;
  cursor: pointer;
  transition: 0.3s;
}

button:hover {
  background: #16325c;
}

.error {
  color: red;
  text-align: center;
}
</style>