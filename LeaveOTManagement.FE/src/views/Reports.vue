<template>
  <div class="report-page">
    <h2>Dashboard Statistics</h2>

    <!-- KPI CARDS -->
<div class="kpi-container">

  <div class="kpi-card">
    <div class="kpi-title">Total OT Employees</div>
    <div class="kpi-value">{{ totalEmployees }}</div>
  </div>

  <div class="kpi-card">
    <div class="kpi-title">Total OT Hours</div>
    <div class="kpi-value">{{ totalHours }}</div>
  </div>

  <div class="kpi-card">
    <div class="kpi-title">Leave Requests</div>
    <div class="kpi-value">{{ totalLeaves }}</div>
  </div>

</div>

    <div class="chart-container">
      <div class="chart-card">
        <h3>Top 5 Employees - OT Hours</h3>
        <div class="chart-wrapper">
          <canvas id="otChart"></canvas>
        </div>
      </div>

      <div class="chart-card">
        <h3>Leave Trends by Month</h3>
        <div class="chart-wrapper">
          <canvas id="leaveChart"></canvas>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import { Chart } from "chart.js/auto";

export default {
  data() {
    return {
      otChartInstance: null,
      leaveChartInstance: null,

      // KPI
      totalEmployees: 0,
      totalHours: 0,
      totalLeaves: 0
    };
  },

  async mounted() {
    try {
      await this.loadOTChart();
    } catch (err) {
      console.error("Load OT chart failed", err);
    }

    try {
      await this.loadLeaveChart();
    } catch (err) {
      console.error("Load Leave chart failed", err);
    }
  },

  beforeUnmount() {
    if (this.otChartInstance) this.otChartInstance.destroy();
    if (this.leaveChartInstance) this.leaveChartInstance.destroy();
  },

  methods: {

    async loadOTChart() {
      try {

        const token = localStorage.getItem("token");

        const res = await axios.get(
          "https://localhost:7121/api/Report/top-ot",
          {
            headers: { Authorization: `Bearer ${token}` }
          }
        );

        const data = res.data || [];

        // KPI calculation
        this.totalEmployees = data.length;

        this.totalHours = data.reduce(
          (sum, x) => sum + x.totalHours,
          0
        );

        const labels = data.map(x => "User " + x.userId);
        const values = data.map(x => x.totalHours);

        if (this.otChartInstance) {
          this.otChartInstance.destroy();
        }

        this.otChartInstance = new Chart(
          document.getElementById("otChart"),
          {
            type: "bar",
            data: {
              labels,
              datasets: [
                {
                  label: "OT Hours",
                  data: values
                }
              ]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false
            }
          }
        );

      } catch (err) {
        console.error("Error loading OT chart:", err);
      }
    },

    async loadLeaveChart() {

      try {

        const token = localStorage.getItem("token");

        const res = await axios.get(
          "https://localhost:7121/api/Report/leave-trends",
          {
            headers: { Authorization: `Bearer ${token}` }
          }
        );

        const data = res.data || [];

        // KPI calculation
        this.totalLeaves = data.reduce(
          (sum, x) => sum + x.totalLeaves,
          0
        );

        const labels = data.map(x => "Month " + x.month);
        const values = data.map(x => x.totalLeaves);

        if (this.leaveChartInstance) {
          this.leaveChartInstance.destroy();
        }

        this.leaveChartInstance = new Chart(
          document.getElementById("leaveChart"),
          {
            type: "line",
            data: {
              labels,
              datasets: [
                {
                  label: "Total Leaves",
                  data: values,
                  fill: false
                }
              ]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false
            }
          }
        );

      } catch (err) {
        console.error("Error loading leave chart:", err);
      }

    }

  }
};
</script>

<style scoped>

.report-page {
  padding: 30px;
}

/* KPI DASHBOARD */

.kpi-container {
  display: grid;
  grid-template-columns: repeat(3,1fr);
  gap: 20px;
  margin-top: 20px;
}

.kpi-card {
  background: white;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

.kpi-title {
  font-size: 14px;
  color: #6b7280;
}

.kpi-value {
  font-size: 28px;
  font-weight: 700;
  margin-top: 6px;
  color: #111827;
}

/* CHART LAYOUT */

.chart-container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 30px;
  margin-top: 30px;
}

.chart-card {
  background: white;
  padding: 20px;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.chart-card canvas {
  width: 100% !important;
  height: 320px !important;
}

/* RESPONSIVE */

@media (max-width: 1024px) {

  .chart-container {
    grid-template-columns: 1fr;
  }

}

@media (max-width: 768px) {

  .kpi-container{
    grid-template-columns:1fr;
  }

}

@media (max-width: 600px) {

  .report-page {
    padding: 15px;
  }

  .chart-card {
    padding: 15px;
  }

  .chart-card canvas {
    height: 260px !important;
  }

  h2 {
    font-size: 20px;
  }

  h3 {
    font-size: 16px;
  }

}

</style>