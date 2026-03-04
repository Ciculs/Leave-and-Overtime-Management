<template>
  <div class="layout-wrapper">

    <!-- Sidebar -->
    <AppSidebar
      :class="{ open: isSidebarOpen }"
      @click="closeSidebar"
    />

    <!-- Overlay -->
    <div
      v-if="isSidebarOpen"
      class="overlay"
      @click="closeSidebar"
    ></div>

    <div class="main-container">
      <AppHeader @toggle-sidebar="toggleSidebar" />

      <main class="content-area">
        <div class="blue-banner">
          <h3>Leave & Overtime Management Platform</h3>
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
import { ref } from "vue"
import AppSidebar from "@/components/AppSidebar.vue"
import AppHeader from "@/components/AppHeader.vue"

const isSidebarOpen = ref(false)

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const closeSidebar = () => {
  isSidebarOpen.value = false
}
</script>

<style scoped>

/* BASE LAYOUT */

.layout-wrapper {
  min-height: 100vh;
  background: #f4f7fe;
}

/* MAIN */
.main-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

/* CONTENT */
.content-area {
  padding: 30px 40px;
  flex: 1;
}

/* BLUE BANNER */
.blue-banner {
  background: linear-gradient(135deg, #4299e1 0%, #3182ce 100%);
  padding: 25px;
  border-radius: 20px;
  color: white;
  margin-bottom: 30px;
}

/* FOOTER */
.footer {
  padding: 20px;
  text-align: center;
  color: #a3aed0;
  font-size: 14px;
}

/* DESKTOP (>=1024px) */

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

/* MOBILE (<1024px) */

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
    background: rgba(0, 0, 0, 0.4);
    z-index: 900;
  }

  .content-area {
    padding: 0px 20px;
  }

  .blue-banner {
    padding: 30px;
    border-radius: 15px;
  }
}

/* SMALL MOBILE (<768px) */

@media (max-width: 768px) {

  .content-area {
    padding: 20px 15px;
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

</style>