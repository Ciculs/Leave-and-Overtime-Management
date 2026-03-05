<template>

<div class="ot-container">

<!-- HEADER -->
<div class="page-header">

<h2>My OT Requests</h2>

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


<!-- CARD GRID -->

<div class="ot-list">

<div
v-for="ot in filteredOT"
:key="ot.id"
class="ot-card"
@click="openDetail(ot)"
>

<div class="ot-header">

<span class="ot-date">
📅 {{ formatDate(ot.details?.[0]?.workDate) }}
</span>

<span :class="['status', ot.status?.toLowerCase()]">
{{ ot.status }}
</span>

</div>

<div class="ot-hours">
⏱ {{ calculateHours(ot.details?.[0]) }} hours
</div>

<p class="ot-reason">
{{ ot.reason }}
</p>

<div class="view-detail">
Tap to view detail →
</div>

</div>

<p v-if="filteredOT.length===0" class="empty">
No OT Requests
</p>

</div>

</div>


<!-- ================= MODAL ================= -->

<div
v-if="selectedOT"
class="modal-overlay"
@click.self="selectedOT=null"
>

<div class="modal-card">

<h3>OT Request Detail</h3>

<p>
<strong>Date:</strong>
{{ formatDate(selectedOT.details?.[0]?.workDate) }}
</p>

<p>
<strong>Hours:</strong>
{{ calculateHours(selectedOT.details?.[0]) }}
</p>

<p>
<strong>Status:</strong>
{{ selectedOT.status }}
</p>

<div class="reason-section">

<strong>Reason:</strong>

<div class="reason-box">
{{ selectedOT.reason }}
</div>

</div>

<button
class="edit-btn"
@click="editOT(selectedOT.id)"
>
Edit
</button>

<button
class="close-btn"
@click="selectedOT=null"
>
Close
</button>

</div>

</div>


<!-- ================= REGISTER ================= -->

<transition name="slide-fade">

<div v-if="showRegister" class="register-wrapper">

<OTRequest @success="handleSuccess"/>

<button
class="cancel-btn"
@click="showRegister=false"
>
← Back to List
</button>

</div>

</transition>

</div>

</template>


<script setup>

import {ref,computed,onMounted} from "vue"
import {useRouter} from "vue-router"
import api from "@/services/api"
import OTRequest from "./OTRequest.vue"

const router = useRouter()

const showRegister = ref(false)
const ots = ref([])
const selectedOT = ref(null)
const selectedStatus = ref("")

onMounted(loadOT)

async function loadOT(){
const res = await api.get("/OT")
ots.value = res.data
}

const filteredOT = computed(()=>{
if(!selectedStatus.value) return ots.value
return ots.value.filter(x=>x.status===selectedStatus.value)
})

function openDetail(ot){
selectedOT.value = ot
}

function editOT(id){
router.push(`/ot/edit/${id}`)
}

function formatDate(date){
if(!date) return "-"
return date.split("T")[0]
}

function calculateHours(detail){

if(!detail) return "-"

const [fh,fm] = detail.fromTime.split(":").map(Number)
const [th,tm] = detail.toTime.split(":").map(Number)

let from = fh + fm/60
let to = th + tm/60

if(to < from){
to += 24
}

return (to-from).toFixed(2)

}

async function handleSuccess(){
showRegister.value=false
await loadOT()
}

</script>


<style scoped>

.ot-container{
background:white;
padding:30px;
border-radius:16px;
}


/* HEADER */

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
border:none;
font-weight:600;
cursor:pointer;
}

.register-btn:hover{
opacity:.9;
}


/* FILTER */

.filter-bar{
margin-bottom:20px;
}

.filter-bar select{
padding:10px 15px;
border-radius:10px;
border:1px solid #ddd;
}


/* GRID */

.ot-list{
display:grid;
grid-template-columns:repeat(auto-fill,minmax(300px,1fr));
gap:20px;
}


/* CARD */

.ot-card{

background:#f9fafc;
padding:18px;
border-radius:14px;
border:1px solid #eee;

cursor:pointer;
transition:.25s;

overflow:hidden;

}

.ot-card:hover{
transform:translateY(-3px);
box-shadow:0 6px 16px rgba(0,0,0,.08);
}


/* HEADER */

.ot-header{
display:flex;
justify-content:space-between;
font-weight:600;
}


/* HOURS */

.ot-hours{
margin-top:8px;
font-size:14px;
}


/* REASON */

.ot-reason{

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


/* VIEW DETAIL */

.view-detail{
margin-top:12px;
font-size:12px;
color:#4318ff;
}


/* STATUS */

.status{
padding:4px 10px;
border-radius:12px;
font-size:12px;
}

.pending{
background:#fff5e6;
color:#d97706;
}

.approved{
background:#e6f9f0;
color:#1a9b5c;
}

.rejected{
background:#ffe6e6;
color:#dc2626;
}


/* MODAL */

.modal-overlay{
position:fixed;
inset:0;
background:rgba(0,0,0,.45);
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


/* REASON BOX */

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


/* BUTTON */

.edit-btn{
margin-top:15px;
background:#4318ff;
color:white;
border:none;
padding:8px 14px;
border-radius:8px;
cursor:pointer;
margin-right:10px;
}

.close-btn{
margin-top:15px;
background:#eee;
border:none;
padding:8px 14px;
border-radius:8px;
cursor:pointer;
}


/* EMPTY */

.empty{
text-align:center;
margin-top:20px;
color:#999;
}


/* REGISTER */

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


/* ANIMATION */

.slide-fade-enter-active,
.slide-fade-leave-active{
transition:all .35s ease;
}

.slide-fade-enter-from{
opacity:0;
transform:translateY(-25px);
}

.slide-fade-leave-to{
opacity:0;
transform:translateY(-25px);
}


/* RESPONSIVE */

@media (max-width:1024px){

.ot-list{
grid-template-columns:repeat(2,1fr);
}

}

@media (max-width:768px){

.ot-list{
grid-template-columns:1fr;
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

.modal-card{
width:100%;
padding:20px;
}

.reason-box{
max-height:140px;
}

}

</style>