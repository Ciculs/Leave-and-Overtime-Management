<template>
  <div class="ot-container animate__animated animate__fadeIn">
    <header class="page-header">
      <div>
        <h2 class="fw-bold mb-1">Overtime Management</h2>
        <p class="text-secondary small">Track and manage your extra working hours</p>
      </div>

      <button 
        v-if="!showRegister" 
        class="btn-register shadow-sm" 
        @click="showRegister = true"
      >
        <i class="fas fa-plus-circle me-2"></i>New Request
      </button>
    </header>

    <main v-if="!showRegister">
      <div class="filter-bar d-flex justify-content-between align-items-center mb-4">
        <div class="custom-select-wrapper">
          <i class="fas fa-filter icon"></i>
          <select v-model="selectedStatus" class="form-select border-0 shadow-sm">
            <option value="">All Requests</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>
        <div class="text-secondary small">
          Showing <strong>{{ filteredOT.length }}</strong> requests
        </div>
      </div>

      <div class="ot-grid">
        <transition-group name="list-complete">
          <div
            v-for="ot in filteredOT"
            :key="ot.id"
            class="ot-card"
            @click="openDetail(ot)"
          >
            <div class="card-status-strip" :class="ot.status?.toLowerCase()"></div>
            
            <div class="ot-card-body">
              <div class="d-flex justify-content-between align-items-start mb-3">
                <span class="date-badge">
                  <i class="far fa-calendar-alt me-2"></i>{{ formatDate(ot.details?.[0]?.workDate) }}
                </span>
                <span :class="['status-pill', ot.status?.toLowerCase()]">
                  {{ ot.status }}
                </span>
              </div>

              <div class="hour-display mb-2">
                <span class="h4 fw-bold text-primary">{{ calculateHours(ot.details?.[0]) }}</span>
                <span class="text-secondary ms-1">hrs</span>
              </div>

              <p class="ot-reason-preview">{{ ot.reason }}</p>

              <div class="view-detail-link">
                <span>View Details</span>
                <i class="fas fa-arrow-right"></i>
              </div>
            </div>
          </div>
        </transition-group>
      </div>

      <div v-if="filteredOT.length === 0" class="empty-state">
        <img src="https://cdn-icons-png.flaticon.com/512/7486/7486744.png" alt="empty" />
        <p>No OT requests found matching your criteria.</p>
      </div>
    </main>

    <teleport to="body">
      <transition name="fade">
        <div v-if="selectedOT" class="modal-overlay" @click.self="selectedOT = null">
          <div class="modal-content-card animate__animated animate__zoomIn">
            <div class="modal-header-custom">
              <h4>Request Details</h4>
              <button class="btn-close-modal" @click="selectedOT = null">&times;</button>
            </div>
            
            <div class="modal-body-custom">
              <div class="info-grid">
                <div class="info-item">
                  <label>Date</label>
                  <span>{{ formatDate(selectedOT.details?.[0]?.workDate) }}</span>
                </div>
                <div class="info-item">
                  <label>Duration</label>
                  <span>{{ calculateHours(selectedOT.details?.[0]) }} Hours</span>
                </div>
                <div class="info-item">
                  <label>Status</label>
                  <span :class="['status-pill', selectedOT.status?.toLowerCase()]">{{ selectedOT.status }}</span>
                </div>
              </div>

              <div class="reason-full mt-4">
                <label class="fw-bold mb-2 d-block">Description / Reason</label>
                <div class="reason-text-area">
                  {{ selectedOT.reason }}
                </div>
              </div>
            </div>

            <div class="modal-footer-custom">
              <button class="btn-edit-action" @click="editOT(selectedOT.id)">
                <i class="fas fa-edit me-2"></i>Edit Request
              </button>
              <button class="btn-secondary-custom" @click="selectedOT = null">Close</button>
            </div>
          </div>
        </div>
      </transition>
    </teleport>

    <transition name="slide-up">
      <div v-if="showRegister" class="register-container">
        <OTRequest @success="handleSuccess" />
        <button class="btn-back-list mt-4" @click="showRegister = false">
          <i class="fas fa-chevron-left me-2"></i>Return to List
        </button>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onActivated } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"
import OTRequest from "./OTRequest.vue"

const router = useRouter()

const showRegister = ref(false)
const ots = ref([])
const selectedOT = ref(null)
const selectedStatus = ref("")

onMounted(loadOT)
onActivated(loadOT)

/* LOAD DATA */

async function loadOT() {

  try {

    const res = await api.get("/OT")

    ots.value = res.data || []

  } catch (err) {

    console.error("Failed to fetch OT", err)

  }

}

/* FILTER */

const filteredOT = computed(() => {

  if (!selectedStatus.value) return ots.value

  return ots.value.filter(x => x.status === selectedStatus.value)

})

/* OPEN DETAIL */

const openDetail = (ot) => {

  selectedOT.value = ot

}

/* EDIT */

const editOT = (id) => {

  router.push(`/ot/edit/${id}`)

}

/* FORMAT DATE */

const formatDate = (dateStr) => {

  if (!dateStr) return "N/A"

  const d = new Date(dateStr)

  return d.toLocaleDateString("en-US", {

    month: "short",
    day: "numeric",
    year: "numeric"

  })

}

/* CALCULATE HOURS */

const calculateHours = (detail) => {

  if (!detail?.fromTime || !detail?.toTime) return "0.00"

  const parse = (t) => {

    const [h,m] = t.split(":").map(Number)

    return h + m/60

  }

  const from = parse(detail.fromTime)
  const to = parse(detail.toTime)

  const diff = to < from ? (to + 24) - from : to - from

  return diff.toFixed(2)

}

/* AFTER CREATE SUCCESS */

const handleSuccess = async () => {

  showRegister.value = false

  await loadOT()

}
</script>

<style scoped>
/* GENERAL */
.ot-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
  background: #f8f9fc;
  min-height: 100vh;
}

/* HEADER */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}

.btn-register {
  background: linear-gradient(135deg, #4318ff 0%, #5e35ff 100%);
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 12px;
  font-weight: 600;
  transition: all 0.3s ease;
}

.btn-register:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 20px rgba(67, 24, 255, 0.2);
}

/* FILTER */
.custom-select-wrapper {
  position: relative;
  width: 200px;
}

.custom-select-wrapper .icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #a3b1cc;
  z-index: 1;
}

.custom-select-wrapper select {
  padding-left: 35px;
  border-radius: 10px;
  cursor: pointer;
}

/* GRID & CARDS */
.ot-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.ot-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  position: relative;
  border: 1px solid rgba(0,0,0,0.04);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  cursor: pointer;
}

.ot-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1);
}

.card-status-strip {
  height: 4px;
  width: 100%;
}

.ot-card-body {
  padding: 1.5rem;
}

/* STATUS PILLS */
.status-pill {
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-pill.pending, .card-status-strip.pending { background: #fff7ed; color: #c2410c; }
.status-pill.approved, .card-status-strip.approved { background: #f0fdf4; color: #15803d; }
.status-pill.rejected, .card-status-strip.rejected { background: #fef2f2; color: #b91c1c; }

.date-badge {
  background: #f1f5f9;
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 0.85rem;
  color: #475569;
}

.ot-reason-preview {
  margin-top: 10px;
  font-size: 14px;
  color: #444;
  word-break: break-word; /* Thay cho break-all để tránh cắt ngang từ */
  overflow: hidden;

  /* Bộ 3 thuộc tính thần thánh để cắt dòng */
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2; /* Hiển thị tối đa 2 dòng */
  
  /* Fallback cho các trình duyệt cực cũ không hỗ trợ */
  max-height: 3em; 
  line-height: 1.5em;
}

.view-detail-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #4318ff;
  font-weight: 600;
  font-size: 0.85rem;
}

/* MODAL STYLING */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content-card {
  background: white;
  width: 550px;
  border-radius: 24px;
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);
  overflow: hidden;
}

.modal-header-custom {
  padding: 1.5rem 2rem;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-body-custom {
  padding: 2rem;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 15px;
}

.info-item label {
  display: block;
  font-size: 0.75rem;
  color: #94a3b8;
  text-transform: uppercase;
  margin-bottom: 5px;
}

.info-item span {
  font-weight: 600;
  color: #1e293b;
}

.reason-text-area {
  background: #f8fafc;
  padding: 1rem;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  min-height: 100px;
  color: #475569;
}

.modal-footer-custom {
  padding: 1.5rem 2rem;
  background: #f8fafc;
  display: flex;
  gap: 12px;
}

.btn-edit-action {
  background: #4318ff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 10px;
  font-weight: 600;
}

.btn-secondary-custom {
  background: white;
  border: 1px solid #e2e8f0;
  padding: 10px 20px;
  border-radius: 10px;
}

/* ANIMATIONS */
.list-complete-enter-from,
.list-complete-leave-to {
  opacity: 0;
  transform: translateY(30px);
}

.list-complete-leave-active {
  position: absolute;
}

.slide-up-enter-active {
  transition: all 0.4s ease-out;
}

.slide-up-enter-from {
  opacity: 0;
  transform: translateY(40px);
}

.empty-state {
  text-align: center;
  padding: 4rem 0;
}

.empty-state img {
  width: 120px;
  opacity: 0.5;
  margin-bottom: 1.5rem;
}

/* RESPONSIVE */
@media (max-width: 768px) {
  .ot-container { padding: 1rem; }
  .page-header { flex-direction: column; align-items: stretch; gap: 15px; }
  .info-grid { grid-template-columns: 1fr; gap: 20px; }
}
</style>