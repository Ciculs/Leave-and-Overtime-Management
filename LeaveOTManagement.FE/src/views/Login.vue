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

          <input v-model="username" type="text" placeholder="Username" @focus="isPasswordFocused = false" />

          <input v-model="password" type="password" placeholder="Password" @focus="isPasswordFocused = true"
            @blur="isPasswordFocused = false" />

          <button class="btn-login" @click="login">Log In</button>

          <div class="forgot-wrapper">
            <a href="#" class="forgot-link">Forgotten password?</a>
          </div>

          <hr />

          <button class="btn-register">Create New Account</button>

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

// --- LOGIC ANIMATION TINH CHỈNH ---
const isPasswordFocused = ref(false)
const pupilStyle = ref({ transform: 'translate(0px, 6px)' })

let mouseX = 0
let mouseY = 0
let currentX = 0
let currentY = 0

const handleMouseMove = (event) => {
  mouseX = event.clientX
  mouseY = event.clientY
}

const updateEyePosition = () => {
  if (!isPasswordFocused.value) {
    // Tính toán mục tiêu (target)
    const targetX = (mouseX / window.innerWidth) * 16 - 8
    const targetY = (mouseY / window.innerHeight) * 16 - 8

    // Tạo độ trễ (Lerp) để mắt di chuyển mượt hơn
    currentX += (targetX - currentX) * 0.1
    currentY += (targetY - currentY) * 0.1

    pupilStyle.value = {
      transform: `translate(${currentX}px, ${currentY + 4}px)` // +4 để mặc định nhìn xuống
    }
  }
  requestAnimationFrame(updateEyePosition)
}

onMounted(() => {
  window.addEventListener('mousemove', handleMouseMove)
  updateEyePosition()
})

onUnmounted(() => {
  window.removeEventListener('mousemove', handleMouseMove)
})

const login = async () => {
  error.value = ""
  if (!username.value || !password.value) {
    error.value = "Please enter username and password"
    return
  }
  try {
    const res = await api.post("/Auth/login", {
      username: username.value,
      password: password.value
    })
    const { token, role, fullName, email } = res.data
    localStorage.setItem("token", token)
    localStorage.setItem("role", role)
    localStorage.setItem("fullName", fullName)
    localStorage.setItem("email", email)

    if (role === "Admin" || role === "HR") router.push("/hr-admin")
    else if (role === "Manager") router.push("/manager")
    else if (role === "Employee") router.push("/employee")
    else router.push("/")

  } catch {
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

.login-left {
  flex: 1;
  background-image: url("/images/kkk.png");
  background-size: cover;
  background-position: center;
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
  background: rgba(0, 0, 0, 0.1);
}

.login-right {
  flex: 1;
  background: #f4f7fb;
  display: flex;
  justify-content: center;
  align-items: center;
}

.right-content {
  width: 450px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* --- AVATAR CSS TINH CHỈNH --- */
.avatar-container {
  display: flex;
  justify-content: center;
  margin-bottom: -10px;
  z-index: 10;
}

.avatar-head {
  width: 100px;
  height: 100px;
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
  gap: 18px;
  margin-top: -10px;
}

.eye {
  width: 26px;
  height: 26px;
  background-color: white;
  border-radius: 50%;
  position: relative;
  overflow: hidden;
  /* Để mí mắt không lòi ra */
}

.pupil {
  width: 11px;
  height: 11px;
  background-color: #1e3c72;
  border-radius: 50%;
  position: absolute;
  left: 7.5px;
  top: 7.5px;
}

/* Hiệu ứng chớp mắt */
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
  bottom: -50px;
  display: flex;
  gap: 30px;
  transition: transform 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.hand {
  width: 38px;
  height: 50px;
  background-color: #1fb3b3;
  border-radius: 18px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
}

.hands.hide-eyes {
  transform: translateY(-65px);
}

/* -------------------------- */

.welcome h2 {
  margin: 0;
  font-size: 26px;
  color: #1e3c72;
  text-align: center;
}

.welcome p {
  margin: 5px 0 0 0;
  color: #666;
  font-size: 14px;
  text-align: center;
}

.card {
  background: white;
  padding: 35px;
  border-radius: 20px;
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.06);
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.stats {
  font-size: 13px;
  color: #555;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

input {
  padding: 14px;
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

.btn-login {
  padding: 13px;
  border-radius: 10px;
  border: none;
  background: #34bde7;
  color: white;
  font-weight: bold;
  font-size: 16px;
  cursor: pointer;
  transition: 0.3s;
}

.btn-login:hover {
  background: #2ba9d1;
  transform: translateY(-1px);
}

.forgot-link {
  color: #1fb3b3;
  font-size: 14px;
  text-decoration: none;
  text-align: center;
  display: block;
}

hr {
  border: none;
  border-top: 1px solid #eee;
  margin: 5px 0;
}

.btn-register {
  padding: 13px;
  border-radius: 10px;
  border: none;
  background: #36eeee;
  color: white;
  font-weight: bold;
  font-size: 15px;
  cursor: pointer;
  transition: 0.3s;
}

.btn-register:hover {
  background: #2de2e2;
}

.error {
  color: #ff4d4f;
  text-align: center;
  font-size: 13px;
}
</style>