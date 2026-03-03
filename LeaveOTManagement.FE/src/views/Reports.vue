<template>
  <div class="report-page">
    <h2>Dashboard Statistics</h2>

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
      leaveChartInstance: null
    };
  },

  async mounted() {
    await this.loadOTChart();
    await this.loadLeaveChart();
  },

  beforeUnmount() {
    if (this.otChartInstance) this.otChartInstance.destroy();
    if (this.leaveChartInstance) this.leaveChartInstance.destroy();
  },

  methods: {
    async loadOTChart() {
      const token = localStorage.getItem("token");

      const res = await axios.get(
        "https://localhost:7121/api/Report/top-ot",
        {
          headers: { Authorization: `Bearer ${token}` }
        }
      );

      const labels = res.data.map(x => "User " + x.userId);
      const values = res.data.map(x => x.totalHours);

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
    },

    async loadLeaveChart() {
      const token = localStorage.getItem("token");

      const res = await axios.get(
        "https://localhost:7121/api/Report/leave-trends",
        {
          headers: { Authorization: `Bearer ${token}` }
        }
      );

      const labels = res.data.map(x => "Month " + x.month);
      const values = res.data.map(x => x.totalLeaves);

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
    }
  }
};
</script>

<style scoped>

/* PAGE */

.report-page {
  padding: 30px;
}

/* CHART LAYOUT */

.chart-container {
  display: flex;
  gap: 30px;
  margin-top: 20px;
  align-items: stretch;
}

/* CARD */
.chart-card {
  flex: 1;
  background: white;
  padding: 20px;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
}

/* Chart wrapper để khống chế chiều cao */
.chart-card canvas {
  width: 100% !important;
  height: 320px !important; 
}

/* TABLET */

@media (max-width: 1024px) {
  .chart-container {
    flex-direction: column;
  }

  .chart-card canvas {
    height: 300px !important;
  }
}

/* MOBILE */
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