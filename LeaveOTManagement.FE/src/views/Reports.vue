<template>
  <div class="report-page">
    <!-- HEADER -->
    <div class="page-header">
      <div>
        <h2>Reports Dashboard</h2>
        <p class="page-subtitle">
          Monitor OT trends, leave trends, detailed OT records, and payroll export
        </p>
      </div>

      <div class="header-actions">
        <button class="btn btn-success" @click="exportPayroll" :disabled="exporting">
          {{ exporting ? "Exporting..." : "Export Payroll" }}
        </button>
      </div>
    </div>

    <!-- KPI -->
    <div class="kpi-container">
      <div class="kpi-card">
        <div class="kpi-title">Top OT Employees</div>
        <div class="kpi-value">{{ totalEmployees }}</div>
      </div>

      <div class="kpi-card">
        <div class="kpi-title">Top OT Hours</div>
        <div class="kpi-value">{{ formatHours(totalHours) }}</div>
      </div>

      <div class="kpi-card">
        <div class="kpi-title">Approved Leave Requests</div>
        <div class="kpi-value">{{ totalLeaves }}</div>
      </div>
    </div>

    <!-- SUMMARY CHARTS -->
    <div class="chart-grid">
      <div class="card-box">
        <div class="card-header">
          <h3>Top 5 Employees - OT Hours</h3>
        </div>
        <div class="chart-wrapper">
          <canvas ref="topOtChartRef"></canvas>
        </div>
      </div>

      <div class="card-box">
        <div class="card-header">
          <h3>Leave Trends by Month</h3>
        </div>
        <div class="chart-wrapper">
          <canvas ref="leaveChartRef"></canvas>
        </div>
      </div>
    </div>

    <!-- FILTER PANEL -->
    <div class="card-box">
      <div class="card-header filter-header">
        <div>
          <h3>OT Detail Report</h3>
          <p class="section-subtitle">
            Filter OT records by month, year, status, and employee
          </p>
        </div>

        <div class="filter-actions">
          <button class="btn btn-primary" @click="filterReport" :disabled="loadingFilter">
            {{ loadingFilter ? "Loading..." : "Apply Filter" }}
          </button>

          <button class="btn btn-download" @click="downloadReport" :disabled="downloadingCsv">
            {{ downloadingCsv ? "Downloading..." : "Download CSV" }}
          </button>
        </div>
      </div>

      <div class="filters">
        <div class="field">
          <label>Month</label>
          <select v-model="month">
            <option v-for="m in 12" :key="m" :value="m">{{ m }}</option>
          </select>
        </div>

        <div class="field">
          <label>Year</label>
          <select v-model="year">
            <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
          </select>
        </div>

        <div class="field">
          <label>Status</label>
          <select v-model="statusFilter">
            <option value="">All</option>
            <option value="Approved">Approved</option>
            <option value="Pending">Pending</option>
            <option value="ManagerApproved">ManagerApproved</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>

        <div class="field search-field">
          <label>Search User</label>
          <input type="text" v-model="searchUser" placeholder="Search by user id or employee name..." />
        </div>
      </div>
    </div>

    <!-- FILTERED CHART -->
    <div class="card-box">
      <div class="card-header">
        <h3>Filtered OT Hours by User</h3>
      </div>

      <div v-if="filteredReports.length === 0" class="empty-chart">
        No OT data found for selected filter.
      </div>

      <div v-else class="chart-wrapper">
        <canvas ref="filteredOtChartRef"></canvas>
      </div>
    </div>

    <!-- TABLE -->
    <div class="card-box">
      <div class="card-header table-header">
        <div>
          <h3>Filtered OT Records</h3>
          <p class="section-subtitle">
            {{ filteredReports.length }} record(s) found
          </p>
        </div>
      </div>

      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Employee</th>
              <th>Date</th>
              <th>Hours</th>
              <th>Status</th>
            </tr>
          </thead>

          <tbody>
            <tr v-if="paginatedReports.length === 0">
              <td colspan="4" class="empty-cell">No data found</td>
            </tr>

            <tr v-for="r in paginatedReports" :key="`${r.requestId || 'req'}-${r.userId}-${r.date}`" class="table-row">
              <td>👤 {{ getUserLabel(r) }}</td>
              <td>{{ formatDate(r.date) }}</td>
              <td class="hours">{{ formatHours(r.hours) }}</td>
              <td>
                <span class="status-badge" :class="getStatusClass(r.status)">
                  {{ r.status }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="pagination" v-if="filteredReports.length > 0">
        <button @click="prevPage" :disabled="page === 1">Prev</button>
        <span>Page {{ page }} / {{ totalPages }}</span>
        <button @click="nextPage" :disabled="page === totalPages">Next</button>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import Chart from "chart.js/auto";

export default {
  data() {
    const now = new Date();

    return {
      topOtChartInstance: null,
      leaveChartInstance: null,
      filteredChart: null,

      totalEmployees: 0,
      totalHours: 0,
      totalLeaves: 0,

      exporting: false,
      loadingFilter: false,
      downloadingCsv: false,

      month: now.getMonth() + 1,
      year: now.getFullYear(),
      years: [2024, 2025, 2026, 2027, 2028],

      statusFilter: "",
      searchUser: "",

      reports: [],
      page: 1,
      perPage: 5
    };
  },

  computed: {
    filteredReports() {
      let data = [...this.reports];

      if (this.searchUser.trim()) {
        const keyword = this.searchUser.trim().toLowerCase();
        data = data.filter(r => {
          const userId = String(r.userId || "").toLowerCase();
          const fullName = String(r.fullName || "").toLowerCase();
          return userId.includes(keyword) || fullName.includes(keyword);
        });
      }

      if (this.statusFilter) {
        data = data.filter(r => r.status === this.statusFilter);
      }

      return data;
    },

    totalPages() {
      return Math.max(1, Math.ceil(this.filteredReports.length / this.perPage));
    },

    paginatedReports() {
      const start = (this.page - 1) * this.perPage;
      const end = start + this.perPage;
      return this.filteredReports.slice(start, end);
    }
  },

  watch: {
    filteredReports() {
      if (this.page > this.totalPages) {
        this.page = this.totalPages;
      }

      this.scheduleFilteredChartRender();
    }
  },

  async mounted() {
    await this.$nextTick();
    await this.loadTopOTChart();
    await this.loadLeaveChart();
    await this.filterReport();
  },

  beforeUnmount() {
    this.destroyChart("topOtChartInstance");
    this.destroyChart("leaveChartInstance");
    this.destroyChart("filteredChart");
  },

  methods: {
    getAuthHeader() {
      const token = localStorage.getItem("token");
      return {
        Authorization: `Bearer ${token}`
      };
    },

    getUserLabel(record) {
      return record.fullName || `User ${record.userId}`;
    },

    getStatusClass(status) {
      if (status === "Approved") return "approved";
      if (status === "Pending") return "pending";
      if (status === "ManagerApproved") return "ManagerApproved";
      if (status === "Rejected") return "rejected";
      return "default";
    },

    formatDate(date) {
      if (!date) return "";
      return new Date(date).toLocaleDateString("vi-VN");
    },

    formatHours(hours) {
      return Number(hours || 0).toFixed(2);
    },

    getCanvas(refName) {
      const canvas = this.$refs[refName];
      if (!canvas) {
        console.warn(`Canvas ref '${refName}' not found`);
        return null;
      }
      return canvas;
    },

    destroyChart(instanceKey) {
      if (this[instanceKey]) {
        this[instanceKey].destroy();
        this[instanceKey] = null;
      }
    },

    destroyChartByCanvas(canvas) {
      if (!canvas) return;
      const existingChart = Chart.getChart(canvas);
      if (existingChart) {
        existingChart.destroy();
      }
    },

    async safeCreateChart({ refName, instanceKey, config }) {
      await this.$nextTick();

      const canvas = this.getCanvas(refName);
      if (!canvas) return null;

      this.destroyChart(instanceKey);
      this.destroyChartByCanvas(canvas);

      const chart = new Chart(canvas, config);
      this[instanceKey] = chart;
      return chart;
    },

    scheduleFilteredChartRender() {
      this.$nextTick(() => {
        this.renderFilteredChart();
      });
    },

    async loadTopOTChart() {
      try {
        const res = await axios.get(
          "https://localhost:7121/api/Report/top-ot",
          {
            headers: this.getAuthHeader()
          }
        );

        const data = res.data || [];

        this.totalEmployees = data.length;
        this.totalHours = data.reduce((sum, x) => sum + Number(x.totalHours || 0), 0);

        const labels = data.map(x => x.fullName || `User ${x.userId}`);
        const values = data.map(x => Number(x.totalHours || 0));

        await this.safeCreateChart({
          refName: "topOtChartRef",
          instanceKey: "topOtChartInstance",
          config: {
            type: "bar",
            data: {
              labels,
              datasets: [
                {
                  label: "OT Hours",
                  data: values,
                  backgroundColor: "#6366f1"
                }
              ]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { display: false }
              }
            }
          }
        });
      } catch (err) {
        console.error("Load OT chart failed:", err);
        window.$toast?.("Load OT chart failed", "error");
      }
    },

    async loadLeaveChart() {
      try {
        const res = await axios.get(
          "https://localhost:7121/api/Report/leave-trends",
          {
            headers: this.getAuthHeader()
          }
        );

        const data = res.data || [];
        this.totalLeaves = data.reduce((sum, x) => sum + Number(x.totalLeaves || 0), 0);

        const labels = data.map(x => `Month ${x.month}`);
        const values = data.map(x => Number(x.totalLeaves || 0));

        await this.safeCreateChart({
          refName: "leaveChartRef",
          instanceKey: "leaveChartInstance",
          config: {
            type: "line",
            data: {
              labels,
              datasets: [
                {
                  label: "Total Leaves",
                  data: values,
                  fill: false,
                  borderColor: "#22c55e",
                  backgroundColor: "#22c55e"
                }
              ]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false
            }
          }
        });
      } catch (err) {
        console.error("Load leave chart failed:", err);
        window.$toast?.("Load leave chart failed", "error");
      }
    },

    async filterReport() {
      try {
        this.loadingFilter = true;

        const res = await axios.get(
          `https://localhost:7121/api/Report/filter?month=${this.month}&year=${this.year}`,
          {
            headers: this.getAuthHeader()
          }
        );

        this.reports = Array.isArray(res.data) ? res.data : [];
        this.page = 1;

        this.scheduleFilteredChartRender();
      } catch (err) {
        console.error("Filter report failed:", err);
        window.$toast?.("Filter report failed", "error");
      } finally {
        this.loadingFilter = false;
      }
    },

    async downloadReport() {
      try {
        this.downloadingCsv = true;

        const res = await axios.get(
          `https://localhost:7121/api/Report/download?month=${this.month}&year=${this.year}`,
          {
            headers: this.getAuthHeader(),
            responseType: "blob"
          }
        );

        const url = window.URL.createObjectURL(new Blob([res.data]));
        const link = document.createElement("a");
        link.href = url;
        link.download = `OT_Report_${this.month}_${this.year}.csv`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);

        window.$toast?.("CSV downloaded successfully", "success");
      } catch (err) {
        console.error("Download CSV failed:", err);
        window.$toast?.("Download CSV failed", "error");
      } finally {
        this.downloadingCsv = false;
      }
    },

    async exportPayroll() {
      try {
        this.exporting = true;

        const res = await axios.get(
          "https://localhost:7121/api/Payroll/export",
          {
            responseType: "blob",
            headers: this.getAuthHeader()
          }
        );

        const file = new Blob([res.data], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        });

        const url = window.URL.createObjectURL(file);
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", "Payroll.xlsx");
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);

        window.$toast?.("Payroll exported successfully", "success");
      } catch (err) {
        console.error("Export payroll failed:", err);
        window.$toast?.("Export payroll failed", "error");
      } finally {
        this.exporting = false;
      }
    },

    async renderFilteredChart() {
      const canvas = this.getCanvas("filteredOtChartRef");

      this.destroyChart("filteredChart");

      if (!this.filteredReports.length) {
        if (canvas) {
          this.destroyChartByCanvas(canvas);
        }
        return;
      }

      await this.$nextTick();

      const freshCanvas = this.getCanvas("filteredOtChartRef");
      if (!freshCanvas) return;

      this.destroyChartByCanvas(freshCanvas);

      const map = {};

      this.filteredReports.forEach(r => {
        const key = this.getUserLabel(r);
        if (!map[key]) {
          map[key] = 0;
        }
        map[key] += Number(r.hours || 0);
      });

      const labels = Object.keys(map);
      const data = Object.values(map);

      this.filteredChart = new Chart(freshCanvas, {
        type: "bar",
        data: {
          labels,
          datasets: [
            {
              label: "OT Hours",
              data,
              backgroundColor: "#6366f1"
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          animation: false,
          plugins: {
            legend: { display: false }
          }
        }
      });
    },

    prevPage() {
      if (this.page > 1) {
        this.page--;
      }
    },

    nextPage() {
      if (this.page < this.totalPages) {
        this.page++;
      }
    }
  }
};
</script>

<style scoped>
.report-page {
  padding: 30px;
  background: #f4f7fe;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 20px;
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0;
  color: #2b3674;
}

.page-subtitle {
  margin-top: 6px;
  color: #707eae;
  font-size: 14px;
}

.header-actions {
  display: flex;
  gap: 10px;
}

.kpi-container {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
  margin-bottom: 10px;
}

.kpi-card,
.card-box {
  background: white;
  border-radius: 18px;
  padding: 20px;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
  margin-bottom: 24px;
}

.kpi-title {
  font-size: 14px;
  color: #707eae;
}

.kpi-value {
  font-size: 28px;
  font-weight: 700;
  margin-top: 8px;
  color: #111827;
}

.chart-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 25px;
}

.chart-wrapper {
  margin-top: 12px;
  padding: 10px 6px;
}

.chart-wrapper canvas {
  width: 100% !important;
  height: 300px !important;
}

.empty-chart {
  min-height: 320px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  font-size: 15px;
  border: 1px dashed #dbe2ea;
  border-radius: 14px;
  background: #f8fbff;
  margin-top: 12px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.card-header h3 {
  margin: 0;
  color: #2b3674;
}

.section-subtitle {
  margin-top: 6px;
  color: #707eae;
  font-size: 14px;
}

.filter-header {
  align-items: flex-start;
  margin-bottom: 10px;
}

.filter-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.filters {
  display: grid;
  grid-template-columns: 140px 140px 180px 1fr;
  gap: 16px;
  margin-top: 20px;
}

.filtered-chart-area {
  margin-top: 18px;
}

.filtered-chart-wrapper {
  padding: 10px;
  border-radius: 14px;
  background: #f9fbff;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.field label {
  font-size: 13px;
  font-weight: 600;
  color: #2b3674;
}

.field select,
.field input {
  height: 42px;
  border: 1px solid #dbe2ea;
  border-radius: 12px;
  padding: 0 12px;
  outline: none;
  background: #fff;
}

.search-field {
  min-width: 220px;
}

.table-header {
  margin-bottom: 14px;
}

.table-wrapper {
  overflow-x: auto;
  margin-top: 10px;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: #f8fafc;
}

th,
td {
  padding: 14px;
  text-align: left;
  border-bottom: 1px solid #edf2f7;
}

.table-row:hover {
  background: #f9fbff;
}

.empty-cell {
  text-align: center;
  color: #94a3b8;
  padding: 24px;
}

.hours {
  font-weight: 700;
  color: #4318ff;
}

.status-badge {
  display: inline-block;
  padding: 6px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.status-badge.approved {
  background: #dcfce7;
  color: #166534;
}

.status-badge.pending {
  background: #fef3c7;
  color: #92400e;
}

.status-badge.ManagerApproved {
  background: #dbeafe;
  color: #1d4ed8;
}

.status-badge.rejected {
  background: #fee2e2;
  color: #991b1b;
}

.status-badge.default {
  background: #e5e7eb;
  color: #374151;
}

.pagination {
  margin-top: 20px;
  display: flex;
  justify-content: center;
  gap: 20px;
  align-items: center;
}

.pagination button,
.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.pagination button {
  background: #6366f1;
  color: white;
}

.pagination button:disabled,
.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #6366f1;
  color: white;
}

.btn-success {
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
}

.btn-download {
  background: #10b981;
  color: white;
}

@media (max-width: 1024px) {

  .chart-grid,
  .kpi-container {
    grid-template-columns: 1fr;
  }

  .filters {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 768px) {
  .report-page {
    padding: 16px;
  }

  .page-header,
  .filter-header {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions,
  .filter-actions {
    width: 100%;
  }

  .header-actions .btn,
  .filter-actions .btn {
    width: 100%;
  }

  .filters {
    grid-template-columns: 1fr;
  }

  .chart-wrapper canvas {
    height: 260px !important;
  }

  .empty-chart {
    min-height: 260px;
  }
}
</style>