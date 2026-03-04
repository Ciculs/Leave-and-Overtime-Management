<template>
  <div class="leave-container">

    <div class="page-header">

  <h2>My Leave Requests</h2>

  <router-link to="/leave/new" class="register-btn">
    + Register Leave Request
  </router-link>

</div>

    <div class="leave-list">

      <div
        v-for="leave in leaves"
        :key="leave.id"
        class="leave-card"
        @click="openDetail(leave)"
      >

        <div class="leave-header">
          <span class="leave-type">{{ leave.leaveType }}</span>

          <span :class="['status', leave.status.toLowerCase()]">
            {{ leave.status }}
          </span>
        </div>

        <div class="leave-date">
          📅 {{ formatDate(leave.fromDate) }} → {{ formatDate(leave.toDate) }}
        </div>

        <div class="leave-days">
          {{ leave.totalDays }} day(s)
        </div>

        <p class="leave-reason">
          {{ leave.reason }}
        </p>

        <div class="view-detail">
          Tap to view detail →
        </div>

      </div>

    </div>


    <!-- MODAL -->
    <div
      v-if="selectedLeave"
      class="modal-overlay"
      @click.self="selectedLeave = null"
    >

      <div class="modal-card">

        <h3>Leave Request Detail</h3>

        <p><strong>Type:</strong> {{ selectedLeave.leaveType }}</p>
        <p><strong>From:</strong> {{ formatDate(selectedLeave.fromDate) }}</p>
        <p><strong>To:</strong> {{ formatDate(selectedLeave.toDate) }}</p>
        <p><strong>Total Days:</strong> {{ selectedLeave.totalDays }}</p>
        <p><strong>Status:</strong> {{ selectedLeave.status }}</p>

        <div class="reason-section">
          <strong>Reason:</strong>

          <div class="reason-box">
            {{ selectedLeave.reason }}
          </div>
        </div>

        <button class="close-btn" @click="selectedLeave=null">
          Close
        </button>

      </div>

    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import api from "@/services/api"

const leaves = ref([])
const selectedLeave = ref(null)

const formatDate = (date) => {
  return new Date(date).toLocaleDateString("vi-VN")
}

const openDetail = (leave) => {
  selectedLeave.value = leave
}

onMounted(async () => {
  const res = await api.get("/Leave/my")
  leaves.value = res.data
})
</script>

<style scoped>

.leave-container{
  background:white;
  padding:30px;
  border-radius:16px;
}
.page-header{
display:flex;
justify-content:space-between;
align-items:center;
margin-bottom:20px;
}

/* BUTTON */

.register-btn{
background:linear-gradient(135deg,#6a5cff,#4318ff);
color:white;
padding:10px 18px;
border-radius:10px;
font-weight:600;
text-decoration:none;
transition:0.2s;
}

.register-btn:hover{
opacity:0.9;
}

h2{
  margin-bottom:20px;
}


/* ====================== */
/* GRID LIST */
/* ====================== */

.leave-list{
  display:grid;
  grid-template-columns:repeat(auto-fill,minmax(300px,1fr));
  gap:20px;
}


/* ====================== */
/* CARD */
/* ====================== */

.leave-card{
  background:#f9fafc;
  padding:18px;
  border-radius:14px;
  border:1px solid #eee;
  cursor:pointer;

  transition:0.25s;

  overflow:hidden;
}

.leave-card:hover{
  transform:translateY(-3px);
  box-shadow:0 6px 16px rgba(0,0,0,0.08);
}

.leave-header{
  display:flex;
  justify-content:space-between;
  font-weight:600;
}

.leave-type{
  color:#2b3674;
}

.leave-date{
  margin-top:8px;
  font-size:14px;
}

.leave-days{
  font-size:13px;
  color:#666;
}


/* FIX REASON PREVIEW */

.leave-reason{
  margin-top:10px;
  font-size:14px;
  color:#444;

  word-break:break-all;
  overflow-wrap:anywhere;

  display:-webkit-box;
  -webkit-line-clamp:2;
  -webkit-box-orient:vertical;

  overflow:hidden;
}


.view-detail{
  margin-top:12px;
  font-size:12px;
  color:#4318ff;
}


/* ====================== */
/* STATUS */
/* ====================== */

.status{
  padding:4px 10px;
  border-radius:12px;
  font-size:12px;
}

.approved{
  background:#e6f9f0;
  color:#1a9b5c;
}

.pending{
  background:#fff5e6;
  color:#d97706;
}

.rejected{
  background:#ffe6e6;
  color:#dc2626;
}


/* ====================== */
/* MODAL */
/* ====================== */

.modal-overlay{
  position:fixed;
  inset:0;
  background:rgba(0,0,0,0.45);
  display:flex;
  align-items:center;
  justify-content:center;
  padding:20px;
}

.modal-card{
  background:white;
  padding:30px;
  border-radius:16px;

  width:650px;
  max-width:90vw;

  max-height:80vh;
  overflow:hidden;
}


/* REASON BOX SCROLL */

.reason-section{
  margin-top:10px;
}

.reason-box{
  margin-top:8px;
  padding:12px;

  border:1px solid #ddd;
  border-radius:8px;

  background:#fafafa;

  max-height:180px;
  overflow-y:auto;

  word-break:break-all;
}


.close-btn{
  margin-top:20px;
  background:#4318ff;
  color:white;
  border:none;
  padding:10px 16px;
  border-radius:8px;
  cursor:pointer;
}


/* ====================== */
/* TABLET */
/* ====================== */

@media (max-width:1024px){

.leave-list{
grid-template-columns:repeat(2,1fr);
}

}


/* ====================== */
/* MOBILE */
/* ====================== */

@media (max-width:768px){

.leave-container{
padding:20px;
}

.leave-list{
grid-template-columns:1fr;
gap:15px;
}

.modal-card{
width:100%;
padding:20px;
}

.reason-box{
max-height:140px;
}

.page-header{
flex-direction:column;
align-items:flex-start;
gap:10px;
}

.register-btn{
width:100%;
text-align:center;
}

}

</style>