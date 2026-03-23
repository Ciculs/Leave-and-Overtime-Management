<template>
  <div class="login-container">
    <div class="login-left">
      <div class="overlay">
        <h1>Leave & Overtime Management System</h1>
        <p>Enterprise Workforce Platform</p>
      </div>
    </div>

    <div class="login-right">
      <div class="right-content">
        <div class="avatar-container">
          <div class="avatar-head animate-breath">
            <div class="eyes-container">
              <div class="eye">
                <div class="pupil" :style="pupilStyle"></div>
                <div class="eyelid"></div>
              </div>
              <div class="eye">
                <div class="pupil" :style="pupilStyle"></div>
                <div class="eyelid"></div>
              </div>
            </div>
            <div class="hands" :class="{ 'hide-eyes': isPasswordFocused }">
              <div class="hand left"></div>
              <div class="hand right"></div>
            </div>
          </div>
        </div>

        <div class="welcome">
          <h2>Welcome Back 👋</h2>
          <p>Sign in to continue to your dashboard</p>
        </div>

        <div class="card">
          <h3>Sign In</h3>

          <input v-model="username" type="text" placeholder="Username" @focus="isPasswordFocused = false"
            @keyup.enter="login" />

          <input v-model="password" type="password" placeholder="Password" @focus="isPasswordFocused = true"
            @blur="isPasswordFocused = false" @keyup.enter="login" />

          <button class="btn-login" @click="login" :disabled="loading">
            {{ loading ? "Logging in..." : "Log In" }}
          </button>

          <div class="forgot-wrapper">
            <a href="#" class="forgot-link">Forgotten password?</a>
          </div>

          <hr />

          <button class="btn-register" type="button">Create New Account</button>

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
import { ref, onMounted, onUnmounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()
const username = ref("")
const password = ref("")
const error = ref("")
const loading = ref(false)

// --- AVATAR ANIMATION ---
const isPasswordFocused = ref(false)
const pupilStyle = ref({ transform: "translate(0px, 6px)" })

let mouseX = 0
let mouseY = 0
let currentX = 0
let currentY = 0
let animationFrameId = null

const handleMouseMove = (event) => {
  mouseX = event.clientX
  mouseY = event.clientY
}

const updateEyePosition = () => {
  if (!isPasswordFocused.value) {
    const targetX = (mouseX / window.innerWidth) * 16 - 8
    const targetY = (mouseY / window.innerHeight) * 16 - 8

    currentX += (targetX - currentX) * 0.1
    currentY += (targetY - currentY) * 0.1

    pupilStyle.value = {
      transform: `translate(${currentX}px, ${currentY + 4}px)`
    }
  }

  animationFrameId = requestAnimationFrame(updateEyePosition)
}

onMounted(() => {
  window.addEventListener("mousemove", handleMouseMove)
  updateEyePosition()
})

onUnmounted(() => {
  window.removeEventListener("mousemove", handleMouseMove)

  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId)
  }
})

const login = async () => {
  error.value = ""

  if (!username.value.trim() || !password.value.trim()) {
    error.value = "Please enter username and password."
    window.$toast?.("Please enter username and password.", "warning")
    return
  }

  loading.value = true

  try {
    const res = await api.post("/Auth/login", {
      username: username.value.trim(),
      password: password.value
    })

    const { token, role, fullName, email, userId } = res.data

    localStorage.setItem("token", token)
    localStorage.setItem("role", role)
    localStorage.setItem("fullName", fullName || "")
    localStorage.setItem("email", email || "")

    if (userId !== undefined && userId !== null) {
      localStorage.setItem("userId", String(userId))
    }

    window.$toast?.("Login successful.", "success")

    if (role === "Admin" || role === "HR") {
      router.push("/hr-admin")
    } else if (role === "Manager") {
      router.push("/manager")
    } else if (role === "Employee") {
      router.push("/employee")
    } else {
      router.push("/")
    }
  } catch (err) {
    error.value = "Invalid username or password."
    window.$toast?.("Invalid username or password.", "error")
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
*,
*::before,
*::after {
  box-sizing: border-box;
}

.login-container {
  display: flex;
  min-height: 100dvh;
  width: 100%;
  font-family: "Segoe UI", sans-serif;
  overflow: hidden;
  background: #f4f7fb;
}

.login-left,
.login-right {
  flex: 1 1 50%;
  min-width: 0;
}

/* LEFT */
.login-left {
  background-image: url("/images/kkk.png");
  background-size: cover;
  background-position: center;
  position: relative;
}

.overlay {
  color: white;
  min-height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: clamp(24px, 4vw, 48px);
  text-align: center;
  background: rgba(0, 0, 0, 0.1);
}

.overlay h1 {
  margin: 0;
  font-size: clamp(32px, 4vw, 56px);
  line-height: 1.15;
  max-width: 700px;
}

.overlay p {
  margin-top: 14px;
  font-size: clamp(16px, 1.5vw, 24px);
}

/* RIGHT */
.login-right {
  background: #f4f7fb;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 24px;
  overflow-y: auto;
}

.right-content {
  width: min(100%, 440px);
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 8px 0;
}

/* AVATAR */
.avatar-container {
  display: flex;
  justify-content: center;
  margin-bottom: -6px;
  z-index: 10;
}

.avatar-head {
  width: 88px;
  height: 88px;
  background-color: #34bde7;
  border-radius: 50%;
  position: relative;
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

.animate-breath {
  animation: breath 4s ease-in-out infinite;
}

@keyframes breath {

  0%,
  100% {
    transform: scale(1);
  }

  50% {
    transform: scale(1.03);
  }
}

.eyes-container {
  display: flex;
  gap: 16px;
  margin-top: -8px;
}

.eye {
  width: 24px;
  height: 24px;
  background-color: white;
  border-radius: 50%;
  position: relative;
  overflow: hidden;
}

.pupil {
  width: 10px;
  height: 10px;
  background-color: #1e3c72;
  border-radius: 50%;
  position: absolute;
  left: 7px;
  top: 7px;
}

.eyelid {
  position: absolute;
  top: -100%;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: #34bde7;
  animation: blink 5s infinite;
}

@keyframes blink {

  0%,
  96%,
  100% {
    top: -100%;
  }

  98% {
    top: 0%;
  }
}

.hands {
  position: absolute;
  bottom: -44px;
  display: flex;
  gap: 24px;
  transition: transform 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.hand {
  width: 32px;
  height: 44px;
  background-color: #1fb3b3;
  border-radius: 18px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
}

.hands.hide-eyes {
  transform: translateY(-58px);
}

/* CONTENT */
.welcome h2 {
  margin: 0;
  font-size: clamp(24px, 2vw, 32px);
  color: #1e3c72;
  text-align: center;
}

.welcome p {
  margin: 6px 0 0;
  color: #666;
  font-size: 14px;
  text-align: center;
}

.card {
  background: white;
  padding: clamp(22px, 2.2vw, 32px);
  border-radius: 20px;
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.06);
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.card h3 {
  margin: 0 0 4px;
  font-size: clamp(28px, 2vw, 34px);
}

.stats {
  font-size: 13px;
  color: #555;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-inline: 4px;
}

input {
  width: 100%;
  padding: 13px 14px;
  border-radius: 10px;
  border: 1px solid #ddd;
  font-size: 14px;
  transition: 0.2s;
}

input:focus {
  outline: none;
  border-color: #18e3f2;
  box-shadow: 0 0 0 3px rgba(24, 119, 242, 0.05);
}

.btn-login,
.btn-register {
  width: 100%;
  padding: 13px;
  border-radius: 10px;
  border: none;
  color: white;
  font-weight: bold;
  cursor: pointer;
  transition: 0.3s;
}

.btn-login {
  background: #34bde7;
  font-size: 16px;
}

.btn-login:hover:not(:disabled) {
  background: #2ba9d1;
  transform: translateY(-1px);
}

.btn-login:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.forgot-wrapper {
  text-align: center;
}

.forgot-link {
  color: #1fb3b3;
  font-size: 14px;
  text-decoration: none;
}

hr {
  border: none;
  border-top: 1px solid #eee;
  margin: 4px 0;
}

.btn-register {
  background: #36eeee;
  font-size: 15px;
}

.btn-register:hover {
  background: #2de2e2;
}

.error {
  color: #ff4d4f;
  text-align: center;
  font-size: 13px;
}

/* LARGE TABLET / SMALL LAPTOP */
@media (max-width: 1200px) {
  .right-content {
    width: min(100%, 400px);
  }

  .card {
    padding: 24px;
  }

  .overlay h1 {
    font-size: clamp(28px, 3vw, 44px);
  }
}

/* TABLET */
@media (max-width: 992px) {
  .login-container {
    flex-direction: column;
    overflow-y: auto;
  }

  .login-left {
    min-height: 220px;
    flex: 0 0 220px;
  }

  .overlay {
    padding: 24px 20px;
  }

  .overlay h1 {
    font-size: 30px;
    max-width: 500px;
  }

  .overlay p {
    font-size: 15px;
  }

  .login-right {
    flex: 1 1 auto;
    align-items: flex-start;
    padding: 24px 16px 32px;
  }

  .right-content {
    width: min(100%, 520px);
    margin: 0 auto;
  }
}

/* MOBILE */
@media (max-width: 768px) {
  .login-left {
    min-height: 160px;
    flex-basis: 160px;
  }

  .overlay h1 {
    font-size: 24px;
  }

  .overlay p {
    font-size: 14px;
  }

  .login-right {
    padding: 18px 14px 24px;
  }

  .right-content {
    gap: 14px;
  }

  .avatar-head {
    width: 74px;
    height: 74px;
  }

  .eye {
    width: 20px;
    height: 20px;
  }

  .pupil {
    width: 8px;
    height: 8px;
    left: 6px;
    top: 6px;
  }

  .hands {
    bottom: -38px;
    gap: 20px;
  }

  .hand {
    width: 28px;
    height: 38px;
  }

  .hands.hide-eyes {
    transform: translateY(-50px);
  }

  .welcome h2 {
    font-size: 22px;
  }

  .welcome p {
    font-size: 13px;
  }

  .card {
    padding: 20px 16px;
    border-radius: 16px;
  }

  .card h3 {
    font-size: 24px;
  }

  input,
  .btn-login,
  .btn-register {
    font-size: 14px;
  }

  .stats {
    font-size: 12px;
  }
}

/* SMALL MOBILE */
@media (max-width: 480px) {
  .login-left {
    display: none;
  }

  .login-right {
    min-height: 100dvh;
    padding: 16px 12px 24px;
  }

  .right-content {
    width: 100%;
  }

  .card {
    padding: 18px 14px;
  }

  .avatar-head {
    width: 68px;
    height: 68px;
  }
}
</style>