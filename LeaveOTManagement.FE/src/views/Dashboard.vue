<template>
  <div class="dashboard-page">
    <div class="hero-card">
      <div class="hero-content">
        <h1>Welcome back, {{ user.name }}!</h1>
        <p>You are logged in as <span class="badge">{{ user.role }}</span></p>
      </div>
    </div>

    <div class="stats-grid">
      <template v-if="user.role === 'HR'">
        <div class="stat-card">
          <div class="icon blue">💸</div>
          <div class="data">
            <p>Total Unpaid Leave</p>
            <h3>{{ stats.totalUnpaid }} Days</h3>
          </div>
        </div>
        <div class="stat-card">
          <div class="icon purple">⏱️</div>
          <div class="data">
            <p>Total OT Hours</p>
            <h3>{{ stats.totalOTHours }}</h3>
          </div>
        </div>
        <div class="stat-card">
          <div class="icon pink">👤</div>
          <div class="data">
            <p>Active Employees</p>
            <h3>150+</h3>
          </div>
        </div>
      </template>

      <template v-else-if="user.role === 'Employee'">
        <div class="stat-card">
          <div class="icon blue">📅</div>
          <div class="data">
            <p>Annual Leave Balance</p>
            <h3>{{ stats.leaveBalance }} Days</h3>
          </div>
        </div>
        </template>
    </div>

    <div v-if="user.role === 'HR'" class="charts-section">
      <div class="chart-card card">
        <div class="card-header">
          <h3>Top 5 OT Hours (Bar Chart)</h3>
        </div>
        <div class="chart-placeholder">
           <div class="mock-bar-chart"></div>
        </div>
      </div>

      <div class="chart-card card">
        <div class="card-header">
          <h3>Leave Trends (Line Chart)</h3>
        </div>
        <div class="chart-placeholder">
           <div class="mock-line-chart"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: "Dashboard",
  data() {
    return {
      // Thông tin giả lập người dùng
      user: {
        name: "Admin User",
        role: "HR" // Bạn có thể đổi thành 'Employee' hoặc 'Manager' 'HR' để test v-if
      },
      // Các con số thống kê để không bị "trắng trơn"
      stats: {
        totalUnpaid: 15,
        totalOTHours: 120,
        leaveBalance: 12,
        pendingCount: 3
      }
    };
  }
};
</script>

<style scoped>
/* BANNER */
.hero-card {
  background: linear-gradient(135deg, #4318ff 0%, #3182ce 100%);
  border-radius: 20px;
  padding: 40px;
  color: white;
  margin-bottom: 30px;
}
.badge {
  background: rgba(255,255,255,0.2);
  padding: 2px 10px;
  border-radius: 20px;
  font-weight: bold;
}

/* GRID THỐNG KÊ */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}
.stat-card {
  background: white;
  padding: 25px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  gap: 20px;
  box-shadow: 14px 17px 40px 4px rgba(112, 144, 176, 0.08);
}
.icon {
  width: 50px; height: 50px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center; font-size: 24px;
}
.blue { background: #f4f7fe; color: #4318ff; }
.purple { background: #f4f7fe; color: #707eae; }

/* BIỂU ĐỒ */
.charts-section {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}
.card {
  background: white;
  border-radius: 20px;
  padding: 25px;
  box-shadow: 14px 17px 40px 4px rgba(112, 144, 176, 0.08);
}
.chart-placeholder {
  height: 250px;
  background: #f8fafc;
  border-radius: 12px;
  margin-top: 20px;
  border: 1px dashed #cbd5e1;
}
</style>