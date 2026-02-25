<template>
  <aside class="sidebar">
    <div class="logo">LeaveOT</div>

    <router-link to="/holidays" class="menu">
      📅 Holidays
    </router-link>

    <router-link to="/leave" class="menu">
      📝 Leave
    </router-link>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 230px;
  background: linear-gradient(180deg, #1e293b, #0f172a);
  color: white;
  padding: 25px 15px;
  display: flex;
  flex-direction: column;
}

.logo {
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 30px;
}

.menu {
  color: #cbd5e1;
  text-decoration: none;
  padding: 10px 12px;
  border-radius: 6px;
  margin-bottom: 8px;
  transition: 0.2s;
}

.menu:hover {
  background: rgba(255, 255, 255, 0.1);
}

.router-link-active {
  background: #1abc9c;
  color: white;
}
</style>