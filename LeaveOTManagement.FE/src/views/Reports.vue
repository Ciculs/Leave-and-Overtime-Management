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

.chart-container {
  display: flex;
  gap: 30px;
  margin-top: 20px;
  align-items: stretch;
}

.chart-card {
  flex: 1;
  background: white;
  padding: 20px;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
}

.chart-card canvas {
  width: 100% !important;
  height: 320px !important;
}

@media (max-width: 1024px) {
  .chart-container {
    flex-direction: column;
  }

  .chart-card canvas {
    height: 300px !important;
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