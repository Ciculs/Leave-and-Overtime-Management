<template>
  <div class="leave-container">
    <div class="page-header">
      <h2>Team Leave Approvals</h2>
    </div>

    <div class="leave-list">
      <div v-for="leave in leaves" :key="leave.id" class="leave-card">
        <div class="leave-header">
          <span class="leave-type">{{ leave.employeeName }} - {{ leave.leaveType }}</span>
          <span class="status pending">{{ leave.status }}</span>
        </div>

        <div class="leave-date">
          {{ formatDate(leave.fromDate) }} - {{ formatDate(leave.toDate) }}
        </div>

        <div class="leave-days">
          {{ leave.totalDays }} days
        </div>

        <p class="leave-reason">
          {{ leave.reason }}
        </p>

        <div class="action-buttons">
          <button class="approve-btn" @click="approveLeave(leave.id)">Approve</button>
          <button class="reject-btn" @click="openRejectModal(leave)">Reject</button>
        </div>
      </div>
    </div>

    <div v-if="selectedLeave" class="modal-overlay" @click.self="selectedLeave = null">
      <div class="modal-card">
        <h3>Reject Leave Request</h3>
        <p>Please provide a reason for rejecting this request.</p>
        <textarea v-model="rejectReason" placeholder="Enter rejection reason"></textarea>
        
        <div class="modal-actions">
          <button class="submit-reject-btn" @click="rejectLeave">Submit Rejection</button>
          <button class="close-btn" @click="selectedLeave = null">Cancel</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import api from "@/services/api"

const leaves = ref([])
const selectedLeave = ref(null)
const rejectReason = ref("")

const loadData = async () => {
  const res = await api.get("/Leave/pending-manager")
  leaves.value = res.data
}

onMounted(() => {
  loadData()
})

const formatDate = (date) => {
  return new Date(date).toLocaleDateString("en-US")
}

const approveLeave = async (id) => {
  try {
    await api.put(`/Leave/manager-approve/${id}`)
    window.$toast("Approved successfully", "success")
    await loadData()
  } catch (error) {
    console.error(error)
    window.$toast("Approve failed. Check F12 Network tab.", "error")
  }
}

const openRejectModal = (leave) => {
  selectedLeave.value = leave
  rejectReason.value = ""
}

const rejectLeave = async () => {
  try {
    await api.put(`/Leave/manager-reject/${selectedLeave.value.id}`, {
      reason: rejectReason.value
    })
    selectedLeave.value = null
    rejectReason.value = ""
    window.$toast("Rejected successfully", "success")
    await loadData()
  } catch (error) {
    console.error(error)
    window.$toast("Reject failed. Check F12 Network tab.", "error")
  }
}
</script>

<style scoped>
.leave-container {
  background: white;
  padding: 30px;
  border-radius: 16px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.leave-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.leave-card {
  background: #f9fafc;
  padding: 18px;
  border-radius: 14px;
  border: 1px solid #eee;
}

.leave-header {
  display: flex;
  justify-content: space-between;
  font-weight: 600;
}

.leave-type {
  color: #2b3674;
}

.status {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
}

.pending {
  background: #fff5e6;
  color: #d97706;
}

.leave-date {
  margin-top: 8px;
  font-size: 14px;
}

.leave-days {
  font-size: 13px;
  color: #666;
}

.leave-reason {
  margin-top: 10px;
  font-size: 14px;
  color: #444;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.action-buttons {
  display: flex;
  gap: 10px;
  margin-top: 15px;
}

.approve-btn {
  background: #10b981;
  color: white;
  border: none;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  flex: 1;
}

.reject-btn {
  background: #ef4444;
  color: white;
  border: none;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  flex: 1;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.modal-card {
  background: white;
  padding: 30px;
  border-radius: 16px;
  width: 400px;
  max-width: 90vw;
}

textarea {
  width: 100%;
  height: 100px;
  margin-top: 10px;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.modal-actions {
  display: flex;
  gap: 10px;
  margin-top: 15px;
}

.submit-reject-btn {
  background: #ef4444;
  color: white;
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  cursor: pointer;
}

.close-btn {
  background: #e5e7eb;
  color: #374151;
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  cursor: pointer;
}
</style>