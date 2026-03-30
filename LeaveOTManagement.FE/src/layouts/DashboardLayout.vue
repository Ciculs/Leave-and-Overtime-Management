<template>
  <div class="layout-wrapper">
    <AppSidebar :class="{ open: isSidebarOpen }" @click="closeSidebar" />

    <div v-if="isSidebarOpen" class="overlay" @click="closeSidebar"></div>

    <div class="main-container">
      <AppHeader @toggle-sidebar="toggleSidebar" />

      <main class="content-area">
        <div v-if="pageTitle || pageSubtitle" class="blue-banner">
          <h2>{{ pageTitle }}</h2>
          <p v-if="pageSubtitle">{{ pageSubtitle }}</p>
        </div>

        <router-view />
      </main>

      <footer class="footer">
        © 2026 LeaveOT System. All rights reserved.
      </footer>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue"
import { useRoute } from "vue-router"
import AppSidebar from "@/components/AppSidebar.vue"
import AppHeader from "@/components/AppHeader.vue"

const route = useRoute()
const isSidebarOpen = ref(false)

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const closeSidebar = () => {
  isSidebarOpen.value = false
}

const pageTitle = computed(() => route.meta.title || "Dashboard")
const pageSubtitle = computed(() => route.meta.subtitle || "")
</script>

<style scoped>
.layout-wrapper {
  min-height: 100vh;
  background: #f4f7fe;
  transition: background 0.3s ease;
}

.main-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  transition: background 0.3s ease;
}

.content-area {
  padding: 30px 40px;
  flex: 1;
  background: transparent;
  transition: background 0.3s ease;
}

.blue-banner {
  background: linear-gradient(135deg, #4299e1 0%, #3182ce 100%);
  padding: 25px;
  border-radius: 20px;
  color: white;
  margin-bottom: 30px;
  box-shadow: 0 16px 35px rgba(49, 130, 206, 0.18);
  transition: background 0.3s ease, box-shadow 0.3s ease;
}

.blue-banner h2 {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
}

.blue-banner p {
  margin: 8px 0 0;
  font-size: 14px;
  opacity: 0.95;
}

.footer {
  padding: 20px;
  text-align: center;
  color: #a3aed0;
  font-size: 14px;
  transition: color 0.3s ease, border-color 0.3s ease, background 0.3s ease;
}

/* DESKTOP */
@media (min-width: 1024px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    width: 280px;
    height: 100vh;
  }

  .main-container {
    margin-left: 280px;
  }
}

/* TABLET / MOBILE SIDEBAR */
@media (max-width: 1023px) {
  .layout-wrapper {
    position: relative;
  }

  .sidebar {
    position: fixed;
    top: 0;
    left: -280px;
    width: 280px;
    height: 100vh;
    transition: 0.3s ease;
    z-index: 1000;
  }

  .sidebar.open {
    left: 0;
  }

  .main-container {
    margin-left: 0;
  }

  .overlay {
    position: fixed;
    inset: 0;
    background: rgba(15, 23, 42, 0.45);
    backdrop-filter: blur(2px);
    z-index: 900;
  }

  .content-area {
    padding: 0 20px;
  }

  .blue-banner {
    padding: 24px;
    border-radius: 15px;
  }

  .blue-banner h2 {
    font-size: 22px;
  }
}

@media (max-width: 768px) {
  .content-area {
    padding: 20px 15px;
  }

  .blue-banner {
    padding: 20px;
    margin-bottom: 20px;
  }

  .blue-banner h2 {
    font-size: 20px;
  }

  .blue-banner p {
    font-size: 14px;
  }

  .footer {
    font-size: 12px;
  }
}

/* DARK MODE */
:global(html.dark-mode) .layout-wrapper {
  background: #0b1220;
}

:global(html.dark-mode) .main-container {
  background: #0b1220;
}

:global(html.dark-mode) .content-area {
  background: transparent;
}

:global(html.dark-mode) .blue-banner {
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
  box-shadow: 0 16px 35px rgba(37, 99, 235, 0.22);
}

:global(html.dark-mode) .footer {
  color: #94a3b8;
  background: transparent;
}

:global(html.dark-mode) .overlay {
  background: rgba(2, 6, 23, 0.65);
}
</style>