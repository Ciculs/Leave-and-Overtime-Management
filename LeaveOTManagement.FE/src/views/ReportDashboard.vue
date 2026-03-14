```vue
<template>
<div class="report-dashboard">

  <div class="page-header">
    <h2>📊 Report Dashboard</h2>
  </div>

  <!-- FILTER CARD -->
  <div class="filter-card">

    <div class="filters">

      <!-- MONTH -->
      <select v-model="month">
        <option value="">Month</option>
        <option v-for="m in 12" :key="m" :value="m">{{ m }}</option>
      </select>

      <!-- YEAR -->
      <select v-model="year">
        <option value="">Year</option>
        <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
      </select>

      <!-- STATUS -->
      <select v-model="statusFilter">
        <option value="">All Status</option>
        <option value="Approved">Approved</option>
        <option value="Pending">Pending</option>
        <option value="WaitingHR">WaitingHR</option>
      </select>

      <!-- SEARCH -->
      <input
        type="text"
        placeholder="Search User..."
        v-model="searchUser"
      />

      <button class="btn filter-btn" @click="filterReport">
        Filter
      </button>

      <button class="btn download-btn" @click="downloadReport">
        Download CSV
      </button>

    </div>

  </div>

  <!-- CHART -->
  <div class="chart-card">
    <h3>Top OT Employees</h3>
    <canvas id="otChart"></canvas>
  </div>

  <!-- TABLE -->
  <div class="table-card">

    <table>

      <thead>
        <tr>
          <th>User</th>
          <th>Date</th>
          <th>Hours</th>
          <th>Status</th>
        </tr>
      </thead>

      <tbody>

        <tr
        v-for="r in paginatedReports"
        :key="r.userId + r.date"
        class="table-row"
        >

          <td>👤 {{ r.userId }}</td>

          <td>{{ formatDate(r.date) }}</td>

          <td class="hours">
            {{ formatHours(r.hours) }}
          </td>

          <td>
            <span
            class="status"
            :class="{
            approved: r.status === 'Approved',
            pending: r.status === 'Pending',
            waiting: r.status === 'WaitingHR'
            }"
            >
            {{ r.status }}
            </span>
          </td>

        </tr>

      </tbody>

    </table>

  </div>

  <!-- PAGINATION -->

  <div class="pagination">

    <button
    @click="prevPage"
    :disabled="page === 1"
    >
    Prev
    </button>

    <span>Page {{ page }} / {{ totalPages }}</span>

    <button
    @click="nextPage"
    :disabled="page === totalPages"
    >
    Next
    </button>

  </div>

</div>
</template>

<script>
import axios from "axios"
import Chart from "chart.js/auto"

export default {

data(){
return{

month:"",
year:"",
reports:[],
years:[2024,2025,2026],

searchUser:"",
statusFilter:"",

page:1,
perPage:5,

chart:null

}
},

computed:{

filteredReports(){

let data = this.reports

if(this.searchUser){

data = data.filter(r =>
String(r.userId).includes(this.searchUser)
)

}

if(this.statusFilter){

data = data.filter(r =>
r.status === this.statusFilter
)

}

return data

},

totalPages(){
return Math.ceil(this.filteredReports.length / this.perPage)
},

paginatedReports(){

const start = (this.page - 1) * this.perPage
const end = start + this.perPage

return this.filteredReports.slice(start,end)

}

},

methods:{

async filterReport(){

if(!this.month || !this.year){
alert("Select month and year")
return
}

const token = localStorage.getItem("token")

const res = await axios.get(
`https://localhost:7121/api/Report/filter?month=${this.month}&year=${this.year}`,
{
headers:{Authorization:`Bearer ${token}`}
}
)

this.reports = res.data

this.page = 1

this.renderChart()

},

async downloadReport(){

if(!this.month || !this.year){
alert("Select month and year")
return
}

const token = localStorage.getItem("token")

const res = await axios.get(
`https://localhost:7121/api/Report/download?month=${this.month}&year=${this.year}`,
{
headers:{Authorization:`Bearer ${token}`},
responseType:"blob"
}
)

const url = window.URL.createObjectURL(new Blob([res.data]))

const link = document.createElement("a")
link.href = url
link.download = "OT_Report.csv"
document.body.appendChild(link)
link.click()

},

renderChart(){

if(this.chart){
this.chart.destroy()
}

const map = {}

this.reports.forEach(r=>{

if(!map[r.userId]){
map[r.userId] = 0
}

map[r.userId] += r.hours

})

const labels = Object.keys(map)
const data = Object.values(map)

this.chart = new Chart(

document.getElementById("otChart"),

{
type:"bar",

data:{
labels:labels,
datasets:[{
label:"OT Hours",
data:data,
backgroundColor:"#6366f1"
}]
},

options:{
responsive:true,
plugins:{
legend:{display:false}
}
}

}

)

},

prevPage(){
if(this.page > 1){
this.page--
}
},

nextPage(){
if(this.page < this.totalPages){
this.page++
}
},

formatDate(date){
return new Date(date).toLocaleDateString()
},

formatHours(hours){
return Number(hours).toFixed(2)
}

}

}
</script>

<style scoped>

.report-dashboard{
padding:30px;
background:#f4f7fe;
min-height:100vh;
}

.page-header h2{
font-weight:700;
margin-bottom:20px;
}

/* FILTER */

.filter-card{
background:white;
padding:20px;
border-radius:14px;
box-shadow:0 4px 14px rgba(0,0,0,0.05);
margin-bottom:25px;
}

.filters{
display:flex;
gap:10px;
flex-wrap:wrap;
}

select,input{
padding:8px 12px;
border-radius:8px;
border:1px solid #e5e7eb;
}

/* BUTTON */

.btn{
border:none;
padding:8px 16px;
border-radius:8px;
cursor:pointer;
}

.filter-btn{
background:#6366f1;
color:white;
}

.download-btn{
background:#10b981;
color:white;
}

/* CHART */

.chart-card{
background:white;
padding:20px;
border-radius:14px;
box-shadow:0 4px 14px rgba(0,0,0,0.05);
margin-bottom:25px;
}

/* TABLE */

.table-card{
background:white;
border-radius:14px;
box-shadow:0 4px 14px rgba(0,0,0,0.05);
overflow:hidden;
}

table{
width:100%;
border-collapse:collapse;
}

th,td{
padding:14px;
}

thead{
background:#f9fafb;
}

.table-row:hover{
background:#f8fafc;
}

/* HOURS */

.hours{
font-weight:700;
color:#2563eb;
}

/* STATUS */

.status{
padding:6px 12px;
border-radius:20px;
font-size:12px;
font-weight:600;
}

.approved{
background:#dcfce7;
color:#166534;
}

.pending{
background:#fef3c7;
color:#92400e;
}

.waiting{
background:#dbeafe;
color:#1e40af;
}

/* PAGINATION */

.pagination{
margin-top:20px;
display:flex;
justify-content:center;
gap:20px;
}

.pagination button{
padding:6px 14px;
border:none;
background:#6366f1;
color:white;
border-radius:6px;
cursor:pointer;
}

.pagination button:disabled{
background:#c7c7c7;
cursor:not-allowed;
}

</style>
```
