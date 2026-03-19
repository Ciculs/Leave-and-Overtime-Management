<template>
  <div class="leave-container">
    <div class="header-actions">
      <h2>My Leave Requests</h2>
      <router-link to="/leave/new" class="new-btn">+ New Request</router-link>
    </div>

    <div class="table-wrapper">
      <table class="leave-table">
        <thead>
          <tr>
            <th>Type</th>
            <th>From Date</th>
            <th>To Date</th>
            <th>Days</th>
            <th>Reason</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="leave in leaves" :key="leave.id">
            <td class="font-medium">{{ leave.leaveType }}</td>
            <td>{{ formatDate(leave.fromDate) }}</td>
            <td>{{ formatDate(leave.toDate) }}</td>
            <td>{{ leave.totalDays }}</td>
            <td class="reason-cell">{{ leave.reason }}</td>
            <td>
              <span :class="['status-badge', sanitizeStatus(leave.status)]">
                {{ leave.status }}
              </span>
            </td>
            <td class="action-cells">
              <template v-if="leave.status.includes('Pending')">
                <button class="edit-btn" @click="openEdit(leave)">✏️ Edit</button>
                <button class="cancel-btn" @click="cancelLeave(leave.id)">✖ Cancel</button>
              </template>
            </td>
          </tr>
          <tr v-if="leaves.length === 0">
            <td colspan="7" class="text-center no-data">No leave requests found.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <h3>Edit Leave Request</h3>
        
        <div class="form-group">
          <label>Leave Type</label>
          <select v-model="editForm.leaveTypeId" required>
            <option v-for="type in leaveTypes" :key="type.leaveTypeId" :value="type.leaveTypeId">
              {{ type.leaveTypeName }}
            </option>
          </select>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>From Date</label>
            <input type="date" v-model="editForm.fromDate" required />
          </div>
          <div class="form-group">
            <label>To Date</label>
            <input type="date" v-model="editForm.toDate" required />
          </div>
        </div>

        <div class="form-group">
          <label>Reason</label>
          <textarea v-model="editForm.reason" rows="3" required></textarea>
        </div>

        <div class="modal-actions">
          <button class="save-btn" @click="submitEdit">Save Changes</button>
          <button class="close-btn" @click="closeModal">Close</button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import api from "@/services/api";

const leaves = ref([]);
const leaveTypes = ref([]);
const showModal = ref(false);

const editForm = ref({
  id: null,
  leaveTypeId: "",
  fromDate: "",
  toDate: "",
  reason: ""
});

const loadData = async () => {
  try {
    const res = await api.get("/Leave/my");
    leaves.value = res.data;
  } catch (error) {
    console.error("Error loading leaves", error);
  }
};

// Gọi API lấy số dư để tiện trích xuất danh sách các Loại phép (Leave Types) cho Modal Edit
const loadLeaveTypes = async () => {
  try {
    const res = await api.get("/Leave/balances");
    const uniqueTypes = [];
    const map = new Map();
    for (const item of res.data) {
      if (!map.has(item.leaveTypeId)) {
        map.set(item.leaveTypeId, true);
        uniqueTypes.push({
          leaveTypeId: item.leaveTypeId,
          leaveTypeName: item.leaveTypeName
        });
      }
    }
    leaveTypes.value = uniqueTypes;
  } catch (error) {
    console.error("Error loading types", error);
  }
};

onMounted(() => {
  loadData();
  loadLeaveTypes();
});

const formatDate = (dateString) => {
  if (!dateString) return "";
  const date = new Date(dateString);
  return date.toLocaleDateString("en-GB");
};

const sanitizeStatus = (status) => {
  if (!status) return "";
  return status.toLowerCase().replace(" ", "-");
};

/* --- US23: CANCEL LEAVE --- */
const cancelLeave = async (id) => {
  if (!confirm("Are you sure you want to cancel this leave request? (Your leave balance will be refunded)")) return;
  try {
    await api.put(`/Leave/cancel/${id}`);
    window.$toast ? window.$toast("Cancelled successfully", "success") : alert("Cancelled successfully");
    await loadData();
  } catch (error) {
    console.error(error);
    const msg = error.response?.data?.message || "Cancel failed";
    window.$toast ? window.$toast(msg, "error") : alert(msg);
  }
};

/* --- US24: EDIT LEAVE --- */
const openEdit = (leave) => {
  // Tìm ID của loại phép dựa trên tên hiển thị
  const typeObj = leaveTypes.value.find(t => t.leaveTypeName === leave.leaveType);
  
  // Format ngày về chuẩn YYYY-MM-DD để hiển thị trên thẻ <input type="date">
  const formatForInput = (dateStr) => {
      const d = new Date(dateStr);
      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
  };

  editForm.value = {
    id: leave.id,
    leaveTypeId: typeObj ? typeObj.leaveTypeId : "",
    fromDate: formatForInput(leave.fromDate),
    toDate: formatForInput(leave.toDate),
    reason: leave.reason
  };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const submitEdit = async () => {
  try {
    await api.put(`/Leave/${editForm.value.id}`, {
      leaveTypeId: editForm.value.leaveTypeId,
      fromDate: editForm.value.fromDate,
      toDate: editForm.value.toDate,
      reason: editForm.value.reason
      // Không cần truyền totalDays, backend sẽ tự động tính toán lại
    });
    window.$toast ? window.$toast("Updated successfully", "success") : alert("Updated successfully");
    closeModal();
    await loadData();
  } catch (error) {
    console.error(error);
    const msg = error.response?.data?.message || "Update failed";
    window.$toast ? window.$toast(msg, "error") : alert(msg);
  }
};
</script>

<style scoped>
.leave-container {
  background: white;
  padding: 30px;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.03);
}

.header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

h2 {
  color: #2b3674;
  margin: 0;
}

.new-btn {
  background-color: #a2d2ff; /* Pastel Blue */
  color: #003049;
  padding: 10px 20px;
  border-radius: 10px;
  text-decoration: none;
  font-weight: 600;
  transition: all 0.2s;
}
.new-btn:hover {
  background-color: #8bbfff;
  transform: translateY(-2px);
}

.table-wrapper {
  overflow-x: auto;
}

.leave-table {
  width: 100%;
  border-collapse: collapse;
}

.leave-table th {
  background-color: #e0fbfc; /* Very Light Blue */
  color: #003049;
  text-align: left;
  padding: 15px;
  font-weight: 600;
  border-bottom: 2px solid #e2e8f0;
}

.leave-table td {
  padding: 15px;
  border-bottom: 1px solid #f1f5f9;
  color: #475569;
  vertical-align: middle;
}

.font-medium {
  font-weight: 600;
  color: #2b3674;
}

.reason-cell {
  max-width: 250px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Status Badges */
.status-badge {
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
}
.status-badge.pending, .status-badge.pending-hr {
  background: #fff5e6;
  color: #d97706;
}
.status-badge.approved {
  background: #dcfce7;
  color: #166534;
}
.status-badge.rejected, .status-badge.cancelled {
  background: #fee2e2;
  color: #991b1b;
}

/* Action Buttons */
.action-cells {
  display: flex;
  gap: 8px;
}

.edit-btn, .cancel-btn {
  border: none;
  padding: 6px 12px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  font-size: 13px;
  transition: all 0.2s;
}

.edit-btn {
  background-color: #b5e48c; /* Pastel Green */
  color: #003049;
}
.edit-btn:hover {
  background-color: #9ce065;
}

.cancel-btn {
  background-color: #ffc8dd; /* Pastel Pink */
  color: #5c001f;
}
.cancel-btn:hover {
  background-color: #ffb0cd;
}

.no-data {
  padding: 40px !important;
  color: #94a3b8;
}

/* Modal Styling */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 100;
}

.modal-content {
  background: white;
  padding: 30px;
  border-radius: 16px;
  width: 500px;
  max-width: 100%;
  box-shadow: 0 10px 25px rgba(0,0,0,0.1);
}

.modal-content h3 {
  margin-top: 0;
  color: #2b3674;
  margin-bottom: 20px;
}

.form-row {
  display: flex;
  gap: 15px;
}

.form-group {
  margin-bottom: 15px;
  flex: 1;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #475569;
  font-size: 14px;
}

.form-group input, .form-group select, .form-group textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-family: inherit;
}

.modal-actions {
  display: flex;
  gap: 10px;
  margin-top: 25px;
}

.save-btn {
  flex: 1;
  background-color: #a2d2ff;
  color: #003049;
  border: none;
  padding: 12px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}

.close-btn {
  flex: 1;
  background-color: #e2e8f0;
  color: #475569;
  border: none;
  padding: 12px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}
</style>