<template>
  <div class="card">
    <div class="card-header">
      <h2>Holiday Management</h2>

      <div class="actions">
        <input type="file" ref="fileInput" @change="handleFile" hidden />
        <button class="btn-secondary" @click="$refs.fileInput.click()">
          📤 Import
        </button>
        <button class="btn-primary" @click="loadData">
          🔄 Refresh
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading">
      Loading data...
    </div>

    <!-- Table -->
    <table v-else-if="holidays.length > 0">
      <thead>
        <tr>
          <th>#</th>
          <th>Date</th>
          <th>Holiday Name</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(h, index) in holidays" :key="h.id">
          <td>{{ index + 1 }}</td>
          <td>{{ formatDate(h.holidayDate) }}</td>
          <td>{{ h.name }}</td>
        </tr>
      </tbody>
    </table>

    <div v-else class="empty">
      No holidays found.
    </div>

    <!-- Toast -->
    <div v-if="toast" class="toast">
      {{ toast }}
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      holidays: [],
      loading: false,
      selectedFile: null,
      toast: null,
    };
  },
  methods: {
    async loadData() {
      this.loading = true;
      try {
        const res = await axios.get("https://localhost:7121/api/Holiday");
        this.holidays = res.data;
      } catch (err) {
        this.showToast("Error loading data");
      }
      this.loading = false;
    },

    async handleFile(event) {
      const file = event.target.files[0];
      if (!file) return;

      const formData = new FormData();
      formData.append("file", file);

      this.loading = true;

      try {
        await axios.post(
          "https://localhost:7121/api/Holiday/import",
          formData,
          {
            headers: {
              "Content-Type": "multipart/form-data",
            },
          }
        );

        this.showToast("Import successful 🎉");
        await this.loadData();
      } catch (err) {
        this.showToast("Import failed ❌");
      }

      this.loading = false;
    },

    formatDate(date) {
      return new Date(date).toLocaleDateString();
    },

    showToast(message) {
      this.toast = message;
      setTimeout(() => {
        this.toast = null;
      }, 2500);
    },
  },

  mounted() {
    this.loadData();
  },
};
</script>

<style scoped>
.card {
  background: white;
  border-radius: 12px;
  padding: 25px;
  box-shadow: 0 6px 25px rgba(0, 0, 0, 0.08);
  position: relative;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.actions {
  display: flex;
  gap: 10px;
}

.btn-primary {
  background: #1abc9c;
  border: none;
  padding: 8px 16px;
  color: white;
  border-radius: 6px;
  cursor: pointer;
  transition: 0.2s;
}

.btn-primary:hover {
  background: #16a085;
}

.btn-secondary {
  background: #3b82f6;
  border: none;
  padding: 8px 16px;
  color: white;
  border-radius: 6px;
  cursor: pointer;
  transition: 0.2s;
}

.btn-secondary:hover {
  background: #2563eb;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: #1e293b;
  color: white;
}

th,
td {
  padding: 12px;
  text-align: left;
}

tbody tr {
  border-bottom: 1px solid #eee;
  transition: 0.2s;
}

tbody tr:hover {
  background: #f1f5f9;
}

.loading {
  text-align: center;
  padding: 30px;
  font-weight: 500;
}

.empty {
  text-align: center;
  padding: 30px;
  color: #888;
}

/* Toast */
.toast {
  position: absolute;
  bottom: 20px;
  right: 20px;
  background: #1abc9c;
  color: white;
  padding: 10px 18px;
  border-radius: 8px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
}
</style>