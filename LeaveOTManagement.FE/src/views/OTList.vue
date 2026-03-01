<template>
  <div class="ot-page">

    <!-- HEADER -->
    <div class="header-ot">
      <h2 class="page-title">My OT Requests</h2>

      <button
        v-if="!showRegister"
        class="register-btn"
        @click="showRegister = true"
      >
        + Register OT
      </button>
    </div>

    <!-- ================= LIST ================= -->
    <div v-if="!showRegister">

      <!-- FILTER -->
      <div class="filter-bar">
        <select v-model="selectedStatus">
          <option value="">All Status</option>
          <option>Pending</option>
          <option>Approved</option>
          <option>Rejected</option>
        </select>
      </div>

      <!-- TABLE -->
      <div class="table-card">
        <table>
          <thead>
            <tr>
              <th>Date</th>
              <th>Hours</th>
              <th>Reason</th>
              <th>Status</th>
              <th width="120">Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="ot in filteredOT" :key="ot.id">

              <!-- Date -->
              <td>
                {{ formatDate(ot.details?.[0]?.workDate) }}
              </td>

              <!-- Hours (auto calculate) -->
              <td>
                {{ calculateHours(ot.details?.[0]) }}
              </td>

              <td>{{ ot.reason }}</td>

              <td>
                <span :class="['status', ot.status.toLowerCase()]">
                  {{ ot.status }}
                </span>
              </td>

              <td>
                <button
                  class="edit-btn"
                  @click="editOT(ot.id)"
                >
                  Edit
                </button>
              </td>
            </tr>

            <tr v-if="filteredOT.length === 0">
              <td colspan="5" class="empty">
                No OT Requests
              </td>
            </tr>

          </tbody>
        </table>
      </div>

    </div>

    <!-- ================= REGISTER FORM ================= -->
    <transition name="slide-fade">
      <div v-if="showRegister" class="register-wrapper">

        <OTRequest @success="handleSuccess" />

        <button
          class="cancel-btn"
          @click="showRegister = false"
        >
          ← Back to List
        </button>

      </div>
    </transition>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"
import OTRequest from "./OTRequest.vue"

const router = useRouter()

const showRegister = ref(false)
const ots = ref([])
const selectedStatus = ref("")

onMounted(loadOT)

async function loadOT() {
  const res = await api.get("/OT")
  ots.value = res.data
}

const filteredOT = computed(() => {
  if (!selectedStatus.value) return ots.value
  return ots.value.filter(
    x => x.status === selectedStatus.value
  )
})

const editOT = (id) => {
  router.push(`/ot/edit/${id}`)
}

/* ===== Format Date ===== */
function formatDate(dateStr) {
  if (!dateStr) return "-"
  return dateStr.split("T")[0]
}

/* ===== Calculate Hours from Time ===== */
function calculateHours(detail) {
  if (!detail) return "-"

  const [fh, fm] = detail.fromTime.split(":").map(Number)
  const [th, tm] = detail.toTime.split(":").map(Number)

  let from = fh + fm / 60
  let to = th + tm / 60

  if (to < from) {
    to += 24 // support OT qua ngày
  }

  return (to - from).toFixed(2)
}

/* ✅ After submit */
async function handleSuccess() {
  showRegister.value = false
  await loadOT()
}
</script>

<style scoped>

.ot-page {
  width: 100%;
}

/* HEADER */
.header-ot{
  display:flex;
  justify-content:space-between;
  align-items:center;
  margin-bottom:20px;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  color: #2b3674;
}

/* REGISTER BUTTON */
.register-btn{
  background: linear-gradient(90deg,#5b2cff,#7b61ff);
  color:white;
  border:none;
  padding:10px 18px;
  border-radius:10px;
  font-weight:600;
  cursor:pointer;
}

.register-btn:hover{
  opacity:.9;
}

/* FILTER */
.filter-bar {
  margin-bottom: 20px;
}

.filter-bar select {
  padding: 10px 15px;
  border-radius: 10px;
  border: 1px solid #ddd;
}

/* TABLE CARD */
.table-card {
  background: white;
  border-radius: 18px;
  padding: 35px 40px;
  width: 100%;
  box-sizing: border-box;
  box-shadow: 0 15px 40px rgba(0,0,0,0.06);
}

table {
  width: 100%;
  border-collapse: collapse;
}

th {
  padding: 14px;
  background: #f4f7fe;
  color: #2b3674;
}

td {
  padding: 14px;
  border-bottom: 1px solid #eee;
}

/* STATUS */
.status {
  padding: 6px 12px;
  border-radius: 20px;
  font-weight: 600;
}

.pending {
  background: #fff3cd;
  color: #856404;
}

.approved {
  background: #d4edda;
  color: #155724;
}

.rejected {
  background: #f8d7da;
  color: #721c24;
}

/* BUTTON */
.edit-btn {
  background: #4318ff;
  color: white;
  border: none;
  padding: 7px 14px;
  border-radius: 8px;
  cursor: pointer;
}

.empty {
  text-align: center;
  padding: 20px;
  color: #999;
}

/* REGISTER AREA */
.register-wrapper{
  background:white;
  padding:25px;
  border-radius:16px;
  box-shadow:0 10px 30px rgba(0,0,0,.05);
}

.cancel-btn{
  margin-top:15px;
  border:none;
  background:#eee;
  padding:8px 14px;
  border-radius:8px;
  cursor:pointer;
}

.slide-fade-enter-active,
.slide-fade-leave-active{
  transition: all .35s ease;
}

.slide-fade-enter-from{
  opacity:0;
  transform:translateY(-25px);
}

.slide-fade-enter-to{
  opacity:1;
  transform:translateY(0);
}

.slide-fade-leave-from{
  opacity:1;
  transform:translateY(0);
}

.slide-fade-leave-to{
  opacity:0;
  transform:translateY(-25px);
}

</style>