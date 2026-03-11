<template>
  <router-view />

  <!-- GLOBAL TOAST -->
  <div class="toast-wrapper">
    <transition-group name="slide">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        :class="['toast-box', toast.type]"
      >
        {{ toast.message }}
      </div>
    </transition-group>
  </div>
</template>

<script setup>
import { ref } from "vue"

const toasts = ref([])

window.$toast = (message, type = "success") => {
  const id = Date.now()

  toasts.value.push({
    id,
    message,
    type
  })

  /* THỜI GIAN HIỂN THỊ THEO LOẠI TOAST */
  const duration =
    type === "error" ? 12000 :
    type === "warning" ? 10000 :
    8000   // success

  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }, duration)
}
</script>

<style>
.toast-wrapper {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.toast-box {
  min-width: 260px;
  padding: 14px 22px;
  color: white;
  font-weight: 600;
  border-radius: 999px;
  box-shadow: 0 8px 20px rgba(0,0,0,0.15);
}

/* SUCCESS */
.toast-box.success {
  background: linear-gradient(135deg,#22c55e,#16a34a);
}

/* ERROR */
.toast-box.error {
  background: linear-gradient(135deg,#ef4444,#dc2626);
}

/* WARNING */
.toast-box.warning {
  background: linear-gradient(135deg,#f59e0b,#d97706);
}

/* SLIDE ANIMATION */

.slide-enter-active,
.slide-leave-active {
  transition: all 0.4s ease;
}

.slide-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.slide-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
</style>