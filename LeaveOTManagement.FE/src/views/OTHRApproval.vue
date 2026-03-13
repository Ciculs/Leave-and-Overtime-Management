<template>
<div class="ot-container">

<!-- HEADER -->

<header class="page-header">
  <div>
    <h2 class="fw-bold mb-1">HR OT Approval</h2>
    <p class="text-secondary small">
      HR approve or reject overtime requests
    </p>
  </div>
</header>

<main>

<!-- HR STATS -->

<div class="stats-grid">

  <div class="stat-card">
    <div class="stat-number">{{ ots.length }}</div>
    <div class="stat-label">Total Requests</div>
  </div>

  <div class="stat-card">
    <div class="stat-number">{{ pendingCount }}</div>
    <div class="stat-label">Pending</div>
  </div>

  <div class="stat-card">
    <div class="stat-number">{{ approvedCount }}</div>
    <div class="stat-label">Approved</div>
  </div>

  <div class="stat-card">
    <div class="stat-number">{{ rejectedCount }}</div>
    <div class="stat-label">Rejected</div>
  </div>

</div>

<!-- FILTER -->

<div class="filter-bar">

<select v-model="selectedStatus" class="filter-select">
<option value="">All Requests</option>
<option value="Pending">Pending</option>
<option value="HRApproved">Approved</option>
<option value="Rejected">Rejected</option>
</select>

<select v-model="sortType" class="filter-select">
<option value="date-desc">Newest Date</option>
<option value="date-asc">Oldest Date</option>
<option value="hours-desc">Most OT Hours</option>
<option value="hours-asc">Least OT Hours</option>
</select>

<div class="request-count">
Showing <strong>{{ filteredOT.length }}</strong> requests
</div>

</div>


<!-- GRID -->

<div class="ot-grid">

  <div
    v-for="ot in paginatedOT"
    :key="ot.id"
    class="ot-card"
    @click="openDetail(ot)"
  >

    <div class="card-status-strip"
         :class="ot.status?.toLowerCase()">
    </div>

    <div class="ot-card-body">

      <div class="card-header">

        <span class="date">
          {{ formatDate(ot.details?.[0]?.workDate) }}
        </span>

        <span :class="['status-pill',ot.status?.toLowerCase()]">
          {{ ot.status }}
        </span>

      </div>

      <div class="hour-display">

        <span class="hours">
          {{ calculateHours(ot.details?.[0]) }}
        </span>

        <span class="hrs">hrs</span>

      </div>

      <p class="reason">
        {{ ot.reason }}
      </p>

      <div class="view-link">
        Review Request →
      </div>

    </div>

  </div>

</div>


<!-- PAGINATION -->

<div v-if="totalPages>1" class="pagination">

<button
  class="page-btn"
  :disabled="currentPage===1"
  @click="prevPage"
>
←
</button>

<button
  v-for="p in totalPages"
  :key="p"
  class="page-number"
  :class="{active:currentPage===p}"
  @click="currentPage=p"
>
{{ p }}
</button>

<button
  class="page-btn"
  :disabled="currentPage===totalPages"
  @click="nextPage"
>
→
</button>

</div>


<!-- MODAL -->

<teleport to="body">

<div
  v-if="selectedOT"
  class="modal-overlay"
  @click.self="selectedOT=null"
>

<div class="modal-card">

<h3 class="modal-title">
OT Request Detail
</h3>


<!-- INFO GRID -->

<div class="modal-grid">

<div class="info-card">
<label>Date</label>
<p>{{ formatDate(selectedOT.details?.[0]?.workDate) }}</p>
</div>

<div class="info-card">
<label>Duration</label>
<p>{{ calculateHours(selectedOT.details?.[0]) }} Hours</p>
</div>

<div class="info-card">
<label>Status</label>

<span :class="['status-pill',selectedOT.status?.toLowerCase()]">
{{ selectedOT.status }}
</span>

</div>

<div class="info-card full">
<label>Reason</label>
<p>{{ selectedOT.reason }}</p>
</div>

<div
  v-if="selectedOT.status === 'Rejected'"
  class="info-card full reject-reason-card"
>
</div>

</div>


<!-- TIMELINE -->

<div class="timeline">

<div class="timeline-item done">
<div class="dot"></div>
Request Created
</div>

<div
class="timeline-item"
:class="{done:selectedOT.status==='HRApproved'}"
>
<div class="dot"></div>
HR Decision
</div>

</div>


<!-- ACTION -->

<div
v-if="selectedOT.status==='Pending'"
class="modal-actions"
>

<button
class="approve-btn"
@click="hrApprove(selectedOT.id)"
>
Approve
</button>

<button
class="reject-btn"
@click="reject(selectedOT.id)"
>
Reject
</button>

</div>

</div>
</div>
<!-- REJECT MODAL -->

<div v-if="showRejectModal" class="modal-overlay">

<div class="modal-card">

<h3 class="modal-title">Reject OT Request</h3>

<div class="info-card full">
<label>Reject Reason</label>

<textarea
v-model="rejectReason"
class="reject-textarea"
placeholder="Enter reason for rejecting this request..."
></textarea>

</div>

<div class="modal-actions">

<button
class="approve-btn"
style="background:#64748b"
@click="closeReject"
>
Cancel
</button>

<button
class="reject-btn"
@click="confirmReject"
>
Confirm Reject
</button>

</div>

</div>

</div>
</teleport>

</main>

</div>

</template>


<script setup>

import { ref, computed, onMounted } from "vue"
import api from "@/services/api"

const ots = ref([])
const selectedOT = ref(null)
const selectedStatus = ref("")
const sortOrder = ref("desc")

const showRejectModal = ref(false)
const rejectReason = ref("")
const rejectId = ref(null)
const sortType = ref("date-desc")

const currentPage = ref(1)
const itemsPerPage = 6

const loading = ref(false)

const pendingCount = computed(() =>
  ots.value.filter(x => x.status === "Pending").length
)

const approvedCount = computed(() =>
  ots.value.filter(x => x.status === "HRApproved").length
)

const rejectedCount = computed(() =>
  ots.value.filter(x => x.status === "Rejected").length
)
onMounted(loadOT)

async function loadOT(){

try{

loading.value = true

const res = await api.get("/OT/hr-pending")

console.log(res.data)

ots.value = res.data || []

}
catch(err){
console.error("Load HR OT failed",err)
}
finally{
loading.value=false
}

}

const filteredOT = computed(()=>{

currentPage.value = 1

let data = [...ots.value]

/* FILTER STATUS */

if(selectedStatus.value){
data = data.filter(
x => x.status?.toLowerCase() === selectedStatus.value.toLowerCase()
)
}

/* SORT */

data.sort((a,b)=>{

const dateA = new Date(a.details?.[0]?.workDate)
const dateB = new Date(b.details?.[0]?.workDate)

const hourA = a.details?.[0]?.hours || 0
const hourB = b.details?.[0]?.hours || 0

switch(sortType.value){

case "date-asc":
return dateA - dateB

case "date-desc":
return dateB - dateA

case "hours-asc":
return hourA - hourB

case "hours-desc":
return hourB - hourA

default:
return dateB - dateA
}

})
console.log(data)

return data

})

const totalPages = computed(()=>{
return Math.ceil(filteredOT.value.length / itemsPerPage)
})

const paginatedOT = computed(()=>{

const start = (currentPage.value-1)*itemsPerPage
const end = start + itemsPerPage

return filteredOT.value.slice(start,end)

})

const nextPage=()=>{
if(currentPage.value<totalPages.value){
currentPage.value++
}
}

const prevPage=()=>{
if(currentPage.value>1){
currentPage.value--
}
}

const openDetail=(ot)=>{
selectedOT.value=ot
}

async function hrApprove(id){

try{

await api.put(`/OT/${id}/hr-approve`)

await loadOT()

selectedOT.value=null

}
catch(err){
console.error(err)
}

}

function reject(id){
rejectId.value = id
rejectReason.value = ""
showRejectModal.value = true
}

async function confirmReject(){

if(!rejectReason.value.trim()){
alert("Please enter reject reason")
return
}

try{

await api.put(`/OT/${rejectId.value}/reject`,{
reason: rejectReason.value
})

await loadOT()

showRejectModal.value = false
selectedOT.value = null

}
catch(err){
console.error(err)
}

}

function closeReject(){
showRejectModal.value = false
}

const formatDate=(dateStr)=>{

if(!dateStr) return "N/A"

const d = new Date(dateStr)

return d.toLocaleDateString("en-US",{
month:"short",
day:"numeric",
year:"numeric"
})

}

const calculateHours=(detail)=>{

if(!detail?.fromTime || !detail?.toTime) return "0.00"

const parse=(t)=>{
const [h,m]=t.split(":").map(Number)
return h + m/60
}

const from=parse(detail.fromTime)
const to=parse(detail.toTime)

const diff = to < from ? (to+24)-from : to-from

return diff.toFixed(2)

}

</script>


<style scoped>

.ot-container{
max-width:1200px;
margin:auto;
padding:40px;
background:#f8fafc;
min-height:100vh;
}

/* HEADER */

.page-header{
margin-bottom:30px;
}

/* FILTER */

.filter-bar{
display:flex;
align-items:center;
gap:10px;
margin-bottom:30px;
}

.filter-select{
padding:10px 16px;
border-radius:10px;
border:none;
background:white;
box-shadow:0 2px 6px rgba(0,0,0,.05);
}

.request-count{
color:#64748b;
}

/* GRID */

.ot-grid{
display:grid;
grid-template-columns:repeat(auto-fill,minmax(320px,1fr));
gap:24px;
}

/* CARD */

.ot-card{
background:white;
border-radius:18px;
cursor:pointer;
transition:.25s;
overflow:hidden;
border:1px solid #f1f5f9;
}

.ot-card:hover{
transform:translateY(-6px);
box-shadow:0 15px 40px rgba(0,0,0,.08);
}

.card-status-strip{
height:4px;
}

.ot-card-body{
padding:24px;
}

.card-header{
display:flex;
justify-content:space-between;
margin-bottom:12px;
}

.date{
color:#475569;
}

.hour-display{
font-size:26px;
font-weight:700;
color:#2563eb;
margin-bottom:10px;
}

.hrs{
font-size:14px;
color:#64748b;
margin-left:5px;
}

.reason{
color:#475569;
margin-bottom:16px;
}

.view-link{
color:#2563eb;
font-weight:600;
}

/* STATUS */

.status-pill{
padding:6px 14px;
border-radius:20px;
font-size:12px;
font-weight:600;
}

.status-pill.hrapproved{
background:#dcfce7;
color:#15803d;
}

.status-pill.rejected{
background:#fee2e2;
color:#b91c1c;
}

/* PAGINATION */

.pagination{
display:flex;
justify-content:center;
gap:8px;
margin-top:40px;
}

.page-number,
.page-btn{
border:none;
background:white;
padding:8px 12px;
border-radius:8px;
cursor:pointer;
box-shadow:0 1px 4px rgba(0,0,0,.05);
}

.page-number.active{
background:#4f46e5;
color:white;
}

/* MODAL */

.modal-overlay{
position:fixed;
inset:0;
background:rgba(0,0,0,.35);
backdrop-filter:blur(10px);
display:flex;
align-items:center;
justify-content:center;
}

.modal-card{
width:560px;
background:white;
border-radius:18px;
padding:30px;
animation:modalPop .25s ease;
}

@keyframes modalPop{
from{
opacity:0;
transform:scale(.9);
}
to{
opacity:1;
transform:scale(1);
}
}

.modal-title{
font-weight:700;
margin-bottom:20px;
}

/* INFO GRID */

.modal-grid{
display:grid;
grid-template-columns:1fr 1fr;
gap:16px;
}

.info-card{
background:#f8fafc;
padding:14px;
border-radius:10px;
}

.info-card.full{
grid-column:1/3;
}

.info-card label{
font-size:12px;
color:#64748b;
display:block;
margin-bottom:4px;
}

/* TIMELINE */

.timeline{
margin-top:20px;
border-left:2px solid #e2e8f0;
padding-left:14px;
}

.timeline-item{
position:relative;
margin-bottom:12px;
color:#64748b;
}

.timeline-item .dot{
width:10px;
height:10px;
background:#cbd5f5;
border-radius:50%;
position:absolute;
left:-19px;
top:4px;
}

.timeline-item.done{
color:#2563eb;
}

.timeline-item.done .dot{
background:#2563eb;
}

/* ACTION */

.modal-actions{
display:flex;
gap:12px;
margin-top:24px;
}

.approve-btn{
background:#22c55e;
color:white;
border:none;
padding:10px 20px;
border-radius:8px;
}

.reject-btn{
background:#ef4444;
color:white;
border:none;
padding:10px 20px;
border-radius:8px;
}

/* HR DASHBOARD STATS */

.stats-grid{
display:grid;
grid-template-columns:repeat(4,1fr);
gap:20px;
margin-bottom:30px;
}

.stat-card{
background:white;
padding:22px;
border-radius:16px;
border:1px solid #f1f5f9;
box-shadow:0 4px 12px rgba(0,0,0,.04);
transition:.25s;
}

.stat-card:hover{
transform:translateY(-3px);
box-shadow:0 10px 30px rgba(0,0,0,.08);
}

.stat-number{
font-size:32px;
font-weight:700;
color:#0f172a;
margin-bottom:6px;
}

.stat-label{
color:#64748b;
font-size:14px;
}

.reject-textarea{
width:100%;
min-height:90px;
border-radius:8px;
border:1px solid #e2e8f0;
padding:10px;
resize:none;
font-size:14px;
}

.reject-reason{
color:#b91c1c;
font-size:13px;
margin-top:6px;
font-style:italic;
}

.reject-reason-card{
background:#fee2e2;
border:1px solid #fecaca;
}

.filter-select{
padding:10px 16px;
border-radius:12px;
border:none;
background:white;
box-shadow:0 2px 6px rgba(0,0,0,.05);
cursor:pointer;
}

.request-count{
margin-left:auto;
}

</style>