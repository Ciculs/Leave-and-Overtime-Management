<template>

<div class="ot-container">

<!-- HEADER -->

<header class="page-header">

<div class="header-left">
<h2>OT Approval</h2>
<p class="text-secondary">Manager approve or reject overtime requests</p>
</div>

<div class="header-right">

<div class="stat-card">
<div class="stat-number">{{ totalOT }}</div>
<div class="stat-label">Total OT</div>
</div>

<div class="stat-card pending">
<div class="stat-number">{{ pendingOT }}</div>
<div class="stat-label">Pending</div>
</div>

</div>

</header>


<!-- FILTER -->

<div class="filter-bar">

<input
v-model="search"
placeholder="Search employee or reason..."
class="search-input"
/>

<select v-model="selectedStatus">
<option value="">All</option>
<option value="Pending">Pending</option>
<option value="Approved">Approved</option>
<option value="Rejected">Rejected</option>
</select>

<select v-model="sortOrder">
<option value="desc">Newest</option>
<option value="asc">Oldest</option>
</select>

</div>


<!-- GRID -->

<div class="ot-grid">

<div
v-for="ot in paginatedOT"
:key="ot.id"
class="ot-card"
@click="openDetail(ot)"
>

<div class="card-status-strip" :class="ot.status.toLowerCase()"></div>

<div class="ot-card-body">

<!-- USER -->

<div class="user-row">

<img
class="avatar"
:src="`https://ui-avatars.com/api/?name=${ot.employeeName}&background=6366f1&color=fff`"
/>

<div>
<div class="employee-name">
{{ ot.employeeName || "Employee" }}
</div>

<div class="date-text">
{{ formatDate(ot.details?.[0]?.workDate) }}
</div>
</div>

</div>


<!-- HOURS -->

<div class="hour-display">
{{ calculateHours(ot.details?.[0]) }} hrs
</div>


<!-- REASON -->

<p class="reason">
{{ ot.reason }}
</p>


<span class="status-pill" :class="ot.status.toLowerCase()">
{{ ot.status }}
</span>

</div>

</div>

</div>


<!-- PAGINATION -->

<div v-if="totalPages > 1" class="pagination">

<button
class="page-btn"
@click="changePage(currentPage - 1)"
:disabled="currentPage === 1"
>
‹
</button>

<button
v-for="page in totalPages"
:key="page"
class="page-number"
:class="{active: currentPage === page}"
@click="changePage(page)"
>
{{ page }}
</button>

<button
class="page-btn"
@click="changePage(currentPage + 1)"
:disabled="currentPage === totalPages"
>
›
</button>

</div>


<!-- DETAIL MODAL -->

<div
v-if="selectedOT"
class="modal-overlay"
@click="selectedOT=null"
>

<div class="modal-card" @click.stop>

<h3>OT Approval</h3>

<div class="ot-info-grid">

<!-- EMPLOYEE -->

<div class="info-card employee-card">

<div class="info-title">Employee</div>

<div class="employee-row">

<img
class="avatar-lg"
:src="`https://ui-avatars.com/api/?name=${selectedOT.employeeName}`"
/>

<span class="employee-name">
{{ selectedOT.employeeName }}
</span>

</div>

</div>


<!-- REASON -->

<div class="info-card reason-card">

<div class="info-title">Reason</div>

<div class="reason-text">
{{ selectedOT.reason }}
</div>

</div>


<!-- HOURS -->

<div class="info-card hours-card">

<div class="info-title">OT Hours</div>

<div class="hours-text">

{{ calculateHours(selectedOT.details?.[0]) }}

<span>hours</span>

</div>

</div>

</div>


<div class="modal-actions">

<button
class="approve-btn"
:disabled="approving"
@click="approve(selectedOT.id)"
>

<span v-if="approving" class="loader"></span>
<span v-else>Approve</span>

</button>

<button
class="reject-btn"
@click="openReject"
>
Reject
</button>

</div>

</div>

</div>


<!-- REJECT MODAL -->

<div v-if="rejectDialog" class="modal-overlay">

<div class="modal-card reject-modal">

<h3>Reject OT Request</h3>

<p class="reject-text">
Please provide a reason for rejecting this OT request.
</p>

<textarea
v-model="rejectReason"
placeholder="Enter reject reason..."
></textarea>

<div class="modal-actions">

<button
class="reject-btn"
@click="reject"
>
Submit Reject
</button>

<button
class="cancel-btn"
@click="rejectDialog=false"
>
Cancel
</button>

</div>

</div>

</div>

</div>

</template>


<script setup>

import { ref, computed, onMounted, watch } from "vue"
import api from "@/services/api"


/* DATA */

const ots = ref([])
const selectedOT = ref(null)

const search = ref("")
const selectedStatus = ref("")
const sortOrder = ref("desc")

const rejectDialog = ref(false)
const rejectReason = ref("")

const approving = ref(false)


/* PAGINATION */

const currentPage = ref(1)
const itemsPerPage = 6


/* LOAD DATA */

onMounted(loadOT)

async function loadOT(){

const res = await api.get("/OT/pending")

ots.value = res.data || []

}


/* FILTER + SEARCH + SORT */

const filteredOT = computed(()=>{

let data = [...ots.value]

if(selectedStatus.value){
data = data.filter(x => x.status === selectedStatus.value)
}

if(search.value){

const key = search.value.toLowerCase()

data = data.filter(x =>
x.reason?.toLowerCase().includes(key) ||
x.employeeName?.toLowerCase().includes(key)
)

}

data.sort((a,b)=>{

const d1 = new Date(a.details?.[0]?.workDate)
const d2 = new Date(b.details?.[0]?.workDate)

if(sortOrder.value === "asc"){
return d1 - d2   // tăng dần
}

return d2 - d1   // giảm dần

})

return data

})


/* PAGINATION */

const totalPages = computed(()=>{
return Math.ceil(filteredOT.value.length / itemsPerPage)
})


const paginatedOT = computed(()=>{

const start = (currentPage.value - 1) * itemsPerPage
const end = start + itemsPerPage

return filteredOT.value.slice(start,end)

})


function changePage(page){

if(page < 1 || page > totalPages.value) return

currentPage.value = page

}


/* RESET PAGE */

watch([search,selectedStatus,sortOrder],()=>{
currentPage.value = 1
})


/* MODAL */

function openDetail(ot){
selectedOT.value = ot
}

function openReject(){
rejectDialog.value = true
}


/* APPROVE */

async function approve(id){

approving.value = true

await api.put(`/OT/${id}/manager-approve`)

setTimeout(async () => {

await loadOT()

approving.value = false
selectedOT.value = null

},800)

}


/* REJECT */

async function reject(){

if(!rejectReason.value) return

await api.put(`/OT/${selectedOT.value.id}/reject`,{
reason: rejectReason.value
})

rejectDialog.value = false
selectedOT.value = null
rejectReason.value = ""

await loadOT()

}


/* UTIL */

function formatDate(date){

if(!date) return ""

return new Date(date).toLocaleDateString()

}


function calculateHours(d){

if(!d) return "0"

const [h1,m1] = d.fromTime.split(":").map(Number)
const [h2,m2] = d.toTime.split(":").map(Number)

const start = h1 + m1/60
const end = h2 + m2/60

return (end - start).toFixed(2)

}


/* STATS */

const totalOT = computed(()=> ots.value.length)

const pendingOT = computed(()=>{
return ots.value.filter(x => x.status === "Pending").length
})

</script>


<style scoped>

/* CONTAINER */

.ot-container{
max-width:1200px;
margin:auto;
padding:30px;
}


/* HEADER */

.page-header{
display:flex;
justify-content:space-between;
align-items:center;
margin-bottom:25px;
}

.header-left h2{
font-size:30px;
font-weight:700;
}

.header-right{
display:flex;
gap:14px;
}


/* STAT */

.stat-card{
background:white;
padding:14px 22px;
border-radius:12px;
box-shadow:0 6px 18px rgba(0,0,0,.08);
text-align:center;
min-width:90px;
}

.stat-number{
font-size:24px;
font-weight:700;
color:#4f46e5;
}

.stat-label{
font-size:12px;
color:#64748b;
}

.stat-card.pending .stat-number{
color:#f59e0b;
}


/* FILTER */

.filter-bar{
display:flex;
gap:12px;
margin-bottom:20px;
}

.search-input{
flex:1;
padding:10px;
border-radius:16px;
border:1px solid #ddd;
}

.filter-bar select{
padding:10px 16px;
border-radius:16px;
border:1px solid #e5e7eb;
background:white;
}


/* GRID */

.ot-grid{
display:grid;
grid-template-columns:repeat(auto-fill,minmax(280px,1fr));
gap:20px;
}


/* CARD */

.ot-card{
background:white;
border-radius:14px;
box-shadow:0 10px 25px rgba(0,0,0,.08);
cursor:pointer;
transition:.25s;
}

.ot-card:hover{
transform:translateY(-6px);
}

.ot-card-body{
padding:18px;
}


/* USER */

.user-row{
display:flex;
gap:10px;
align-items:center;
margin-bottom:10px;
}

.avatar{
width:40px;
height:40px;
border-radius:50%;
}

.employee-name{
font-weight:600;
}

.date-text{
font-size:12px;
color:#888;
}


/* HOURS */

.hour-display{
font-size:22px;
font-weight:700;
color:#4f46e5;
}

.reason{
margin-top:8px;
color:#555;
}


/* STATUS */

.status-pill{
display:inline-block;
margin-top:10px;
padding:4px 10px;
border-radius:20px;
font-size:12px;
}

.status-pill.pending{background:#fff7ed;color:#c2410c}
.status-pill.approved{background:#ecfdf5;color:#15803d}
.status-pill.rejected{background:#fef2f2;color:#b91c1c}


/* PAGINATION */

.pagination{
display:flex;
justify-content:center;
gap:8px;
margin-top:30px;
}

.page-btn,
.page-number{
width:36px;
height:36px;
border-radius:10px;
border:1px solid #e2e8f0;
background:white;
cursor:pointer;
font-weight:600;
display:flex;
align-items:center;
justify-content:center;
}

.page-number.active{
background:#4f46e5;
color:white;
border-color:#4f46e5;
}


/* MODAL */

.modal-overlay{
position:fixed;
inset:0;
background:rgba(0,0,0,.5);
display:flex;
align-items:center;
justify-content:center;
}

.modal-card{
background:white;
padding:25px;
border-radius:14px;
width:420px;
}


/* BUTTONS */

.modal-actions{
margin-top:15px;
display:flex;
gap:10px;
}

.approve-btn{
background:#16a34a;
color:white;
border:none;
padding:8px 16px;
border-radius:6px;
}

.reject-btn{
background:#dc2626;
color:white;
border:none;
padding:8px 16px;
border-radius:6px;
}

.cancel-btn{
background:#e5e7eb;
border:none;
padding:8px 16px;
border-radius:6px;
}

textarea{
width:100%;
min-height:80px;
padding:10px;
border-radius:8px;
border:1px solid #ddd;
}

.loader{
width:16px;
height:16px;
border:2px solid white;
border-top:2px solid transparent;
border-radius:50%;
animation:spin .7s linear infinite;
}

@keyframes spin{
to{transform:rotate(360deg)}
}

/* GRID */

.ot-info-grid{
display:grid;
grid-template-columns:1fr;
gap:14px;
margin-top:20px;
}


/* CARD */

.info-card{
background:#f8fafc;
border:1px solid #e2e8f0;
border-radius:12px;
padding:16px;
transition:0.25s;
}

.info-card:hover{
transform:translateY(-2px);
box-shadow:0 6px 14px rgba(0,0,0,0.08);
}


/* TITLE */

.info-title{
font-size:12px;
color:#64748b;
margin-bottom:8px;
font-weight:600;
}


/* EMPLOYEE */

.employee-row{
display:flex;
align-items:center;
gap:10px;
}

.employee-name{
font-weight:600;
}


/* REASON */

.reason-text{
color:#444;
line-height:1.4;
}


/* HOURS */

.hours-text{
font-size:24px;
font-weight:700;
color:#4f46e5;
}

.hours-text span{
font-size:14px;
color:#64748b;
margin-left:4px;
}

</style>