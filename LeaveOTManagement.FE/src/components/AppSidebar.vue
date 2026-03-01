<template>
  <aside class="sidebar">
    <div class="brand">LeaveOT</div>

    <nav class="nav-menu">
      <router-link to="/dashboard" class="nav-item">📊 Dashboard</router-link>

      <template v-if="user.role === 'Employee'">
  <router-link to="/leave/new" class="nav-item">
    📝 Submit Leave
  </router-link>

  <router-link to="/my-requests" class="nav-item">
    📁 My Requests
  </router-link>
</template>

      <template v-if="user.role === 'Manager'">
        <router-link to="/manager/approvals" class="nav-item">✅ Team Approvals</router-link>
        <router-link to="/manager/calendar" class="nav-item">📅 Team Calendar</router-link>
      </template>

      <template v-if="user.role === 'HR'">
        <router-link to="/holidays" class="nav-item">📅 Holiday Calendar</router-link>
        <router-link to="/reports" class="nav-item">📈 Reporting & Export</router-link>
      </template>
    </nav>
  </aside>
</template>

<script>
export default {
  name: "AppSidebar",
  data() {
    return {
      // Giả lập User đang đăng nhập. 
      // Sau này bạn sẽ lấy giá trị này từ Pinia store hoặc localStorage
      user: {
        name: "Admin",
        role: "HR" // Thử đổi thành 'Employee' hoặc 'Manager' để thấy menu thay đổi
      }
    };
  }
};
</script>

<style scoped>
.sidebar {
  width: 260px;
  background: white;
  border-right: 1px solid #e2e8f0;
  display: flex;
  flex-direction: column;
  height: 100vh;
}

.brand {
  padding: 30px 24px;
  font-size: 24px;
  font-weight: 800;
  color: #2b3674;
}

.nav-menu {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 0 12px;
}

.nav-item {
  padding: 12px 16px;
  text-decoration: none;
  color: #a3aed0;
  font-weight: 600;
  border-radius: 12px;
  transition: 0.3s;
}

.nav-item:hover {
  background: #f4f7fe;
  color: #2b3674;
}

.router-link-active {
  background: #4318ff !important;
  color: white !important;
}
</style>