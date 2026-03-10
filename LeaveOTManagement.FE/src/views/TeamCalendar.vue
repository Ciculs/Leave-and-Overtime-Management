<script setup>
import { computed, ref, onMounted } from "vue"
import { getTeamCalendar } from "@/services/leaveService"

/* DATA */

const leaves = ref([])

const viewMode = ref("calendar")

const currentDate = ref(new Date())

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const today = new Date().toISOString().split("T")[0]

/* POPUP */

const selectedDate = ref(null)
const selectedLeaves = ref([])
const showPopup = ref(false)

/* LOAD DATA */

const loadLeaves = async () => {
  const res = await getTeamCalendar()
  leaves.value = res.data
}

onMounted(loadLeaves)

/* CALENDAR DATA */

const daysInMonth = computed(() =>
  new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
)

const firstDayOfMonth = computed(() => {
  let day = new Date(currentYear.value, currentMonth.value, 1).getDay()
  return day === 0 ? 6 : day - 1
})

/* BUILD MAP */

const leaveMap = computed(() => {

  const map = {}

  leaves.value.forEach(l => {

    const start = new Date(l.startDate)
    const end = new Date(l.endDate)

    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {

      const key = d.toISOString().split("T")[0]

      if (!map[key]) map[key] = []

      map[key].push({
        name: l.employeeName,
        type: l.leaveType
      })
    }

  })

  return map
})

/* MONTH SWITCH */

const changeMonth = (offset) => {

  currentDate.value = new Date(
    currentYear.value,
    currentMonth.value + offset,
    1
  )
}

/* COLOR */

const leaveColor = (type) => {

  if (type === "Annual Leave") return "#3b82f6"
  if (type === "Sick Leave") return "#ef4444"
  if (type === "Unpaid Leave") return "#6b7280"

  return "#6366f1"
}

/* OPEN DAY */

const openDay = (day) => {

  const key =
    `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`

  selectedDate.value = key
  selectedLeaves.value = leaveMap.value[key] || []

  showPopup.value = true
}

const closePopup = () => showPopup.value = false

</script>

<template>

  <div class="page-card">

    <!-- HEADER -->

    <div class="page-header">


      <div>
        <h2>Team Leave Calendar</h2>
        <p>View approved leave of your team</p>
      </div>

      <div class="view-switch">

        <button :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'">
          List
        </button>

        <button :class="{ active: viewMode === 'calendar' }" @click="viewMode = 'calendar'">
          Calendar
        </button>

      </div>


    </div>

    <!-- CALENDAR -->

    <div v-if="viewMode === 'calendar'" class="calendar">


      <div class="calendar-header">

        <button @click="changeMonth(-1)">◀</button>

        <h3>
          {{ new Date(currentYear, currentMonth).toLocaleString('default', { month: 'long' }) }}
          {{ currentYear }}
        </h3>

        <button @click="changeMonth(1)">▶</button>

      </div>


      <div class="calendar-grid">

        <div class="day-name" v-for="d in ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']" :key="d">
          {{ d }}
        </div>


        <div v-for="n in firstDayOfMonth" :key="'empty-' + n" class="empty-cell"></div>


        <div v-for="day in daysInMonth" :key="day" class="day-cell" @click="openDay(day)" :class="{
          today:
            today ===
            `${currentYear}-${String(currentMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
        }">

          <div class="date-number">
            {{ day }}
          </div>


          <div v-for="leave in leaveMap[
            `${currentYear}-${String(currentMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
          ]" :key="leave.name" class="leave-event" :style="{ background: leaveColor(leave.type) }"
            :title="leave.name + ' - ' + leave.type">
            {{ leave.name }}
          </div>

        </div>

      </div>


    </div>

    <!-- LIST VIEW -->

    <div v-if="viewMode === 'list'" class="list-view">


      <table>

        <thead>
          <tr>
            <th>Employee</th>
            <th>Leave Type</th>
            <th>From</th>
            <th>To</th>
          </tr>
        </thead>

        <tbody>

          <tr v-for="leave in leaves" :key="leave.employeeName">

            <td>{{ leave.employeeName }}</td>
            <td>{{ leave.leaveType }}</td>
            <td>{{ leave.startDate.slice(0, 10) }}</td>
            <td>{{ leave.endDate.slice(0, 10) }}</td>

          </tr>

        </tbody>

      </table>


    </div>

  </div>

  <!-- POPUP -->

  <div v-if="showPopup" class="popup-overlay">

    <div class="popup">


      <h3>Leave on {{ selectedDate }}</h3>

      <div v-if="selectedLeaves.length === 0">
        No leave on this day
      </div>

      <ul v-else>

        <li v-for="l in selectedLeaves" :key="l.name">
          {{ l.name }} - {{ l.type }}
        </li>

      </ul>

      <button @click="closePopup">Close</button>


    </div>

  </div>

</template>

<style scoped>
.page {
  padding: 10px;
}

.page-card{
  background:white;
  padding:24px;
  border-radius:16px;
  box-shadow:0 4px 10px rgba(0,0,0,0.05);
}

/* HEADER */

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.page-header p {
  margin: 4px 0 0;
  color: #707eae;
  font-size: 14px;
}

/* SWITCH */

.view-switch {
  display: flex;
  gap: 10px;
}

.view-switch button {
  border: none;
  padding: 6px 14px;
  border-radius: 8px;
  background: #e5e7eb;
  cursor: pointer;
}

.view-switch button.active {
  background: #4318ff;
  color: white;
}

/* CALENDAR */

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 8px;
}

.day-name {
  text-align: center;
  font-weight: bold;
  color: #707eae;
}

.day-cell {
  background: #f9fbff;
  border-radius: 12px;
  padding: 8px;
  min-height: 90px;
  cursor: pointer;
  transition: 0.2s;
}

.day-cell:hover {
  background: #edf2ff;
}

.today {
  border: 2px solid #4318ff;
  background: #eef2ff;
}

.date-number {
  font-weight: bold;
  margin-bottom: 4px;
}

.leave-event {
  font-size: 11px;
  margin-top: 4px;
  padding: 3px 6px;
  border-radius: 6px;
  color: white;
  display: inline-block;
}

/* LIST */

.list-view table {
  width: 100%;
  border-collapse: collapse;
}

.list-view th,
.list-view td {
  padding: 10px;
  border-bottom: 1px solid #e5e7eb;
  text-align: left;
}

/* POPUP */

.popup-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  justify-content: center;
  align-items: center;
}

.popup {
  background: white;
  padding: 25px;
  border-radius: 12px;
  min-width: 300px;
}

.popup button {
  background: #4318ff;
  color: white;
  border: none;
  padding: 8px 14px;
  border-radius: 6px;
  cursor: pointer;
}
</style>
