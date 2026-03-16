<script setup>
import { computed, ref, onMounted, watch } from "vue"
import { getTeamCalendar } from "@/services/leaveService"

/* DATA */

const leaves = ref([])
const loadedMonths = ref([])

const viewMode = ref("calendar")

const currentDate = ref(new Date())

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => selectedYear.value)

const today = new Date().toISOString().split("T")[0]

/* POPUP */

const selectedDate = ref(null)
const selectedLeaves = ref([])
const showPopup = ref(false)

const selectedYear = ref(new Date().getFullYear())
/* LOAD DATA */

const loadLeaves = async (year, month) => {

  const key = `${year}-${month}`

  if (loadedMonths.value.includes(key)) return

  try {

    const res = await getTeamCalendar(year, month)

    leaves.value = [...leaves.value, ...res.data]

    loadedMonths.value.push(key)

  } catch (err) {

    console.error("Load team calendar error", err)

  }
}

onMounted(() => {

  loadLeaves(currentYear.value, currentMonth.value + 1)

})

watch([currentYear, currentMonth], () => {

  loadLeaves(currentYear.value, currentMonth.value + 1)

})

watch(selectedYear, () => {

  leaves.value = []
  loadedMonths.value = []

  currentDate.value = new Date(selectedYear.value, currentMonth.value, 1)

  loadLeaves(selectedYear.value, currentMonth.value + 1)

})

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

/* TODAY */

const isToday = (day) => {

  const key =
    `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`

  return key === today

}

/* WEEKEND */

const isWeekend = (day) => {

  const d = new Date(currentYear.value, currentMonth.value, day).getDay()

  return d === 0 || d === 6

}

/* DATE KEY */

const getKey = (day) => {

  return `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`

}

/* OPEN DAY */

const openDay = (day) => {

  const key = getKey(day)

  selectedDate.value = key

  selectedLeaves.value = leaveMap.value[key] || []

  showPopup.value = true

}

const closePopup = () => showPopup.value = false

const monthLabel = computed(() => {
  const date = currentDate.value

  if (!date || isNaN(date)) return ""

  return date.toLocaleString("default", {
    month: "long"
  })
})
</script>

<template>

  <div class="page-card">

    <!-- HEADER -->

    <div class="page-header">

      <div>
        <h2>Team Leave Calendar</h2>
        <p>View approved leave of your team</p>
      </div>

      <div class="header-actions">

        <div class="year-filter">
          <label>Year:</label>

          <select v-model="selectedYear">

            <option v-for="y in [2024, 2025, 2026, 2027, 2028]" :key="y" :value="y">
              {{ y }}
            </option>

          </select>

        </div>

        <div class="view-toggle">

          <button :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'">
            List
          </button>

          <button :class="{ active: viewMode === 'calendar' }" @click="viewMode = 'calendar'">
            Calendar
          </button>

        </div>

      </div>

    </div>

    <!-- CALENDAR -->

    <div v-if="viewMode === 'calendar'" class="calendar">

      <div class="calendar-header">

        <button @click="changeMonth(-1)">◀</button>

        <h3>
          {{ monthLabel }} {{ currentYear }}
        </h3>

        <button @click="changeMonth(1)">▶</button>

      </div>

      <!-- ANIMATED GRID -->

      <Transition name="calendar-slide" mode="out-in">
        <div class="calendar-grid" :key="currentMonth">

          <div class="day-name" v-for="d in ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']" :key="d">
            {{ d }}
          </div>

          <div v-for="n in firstDayOfMonth" :key="'empty-' + n" class="empty-cell"></div>

          <div v-for="day in daysInMonth" :key="day" class="day-cell" @click="openDay(day)" :class="{
            today: isToday(day),
            weekend: isWeekend(day),
            hasLeave: leaveMap[getKey(day)]
          }">

            <div class="date-number">{{ day }}</div>

            <div v-for="(leave, i) in (leaveMap[getKey(day)] || []).slice(0, 3)" :key="leave.name + i"
              class="leave-event" :style="{ background: leaveColor(leave.type) }"
              :title="leave.name + ' - ' + leave.type">
              {{ leave.name }}
            </div>

            <div v-if="(leaveMap[getKey(day)] || []).length > 3" class="more-leave">
              +{{ leaveMap[getKey(day)].length - 3 }} more
            </div>

          </div>

        </div>
      </Transition>

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
.page-card {
  background: white;
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
}

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

.view-toggle {
  display: flex;
  background: #f4f7fe;
  border-radius: 12px;
  overflow: hidden;
}

.view-toggle button {
  border: none;
  background: transparent;
  padding: 8px 18px;
  cursor: pointer;
  font-weight: 600;
  color: #707eae;
  transition: 0.3s;
}

.view-toggle button.active {
  background: linear-gradient(135deg, #4318ff 0%, #3182ce 100%);
  color: white;
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.calendar-header button {
  background: #eef2ff;
  border: none;
  padding: 6px 12px;
  border-radius: 8px;
  cursor: pointer;
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
  transform: translateY(-2px);
  background: #edf2ff;
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.08);
}

.today {
  border: 2px solid #4318ff;
  background: #eef2ff;
}

.weekend {
  background: #d8e2ff;
}

.date-number {
  font-weight: bold;
  margin-bottom: 4px;
}

.leave-event {
  font-size: 10px;
  margin-top: 4px;
  padding: 2px 6px;
  border-radius: 4px;
  color: white;
  display: inline-block;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}

.more-leave {
  font-size: 11px;
  color: #6b7280;
  margin-top: 4px;
}

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

.calendar-slide-enter-active,
.calendar-slide-leave-active {
  transition: all .25s ease;
}

.calendar-slide-enter-from {
  opacity: 0;
  transform: translateX(20px);
}

.calendar-slide-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

.hasLeave {
  border-left: 4px solid #4318ff;
}

.more-leave {
  font-size: 10px;
  margin-top: 3px;
  color: #6b7280;
  cursor: pointer;
}

.header-actions{
display:flex;
align-items:center;
gap:20px;
}

.year-filter{
display:flex;
align-items:center;
gap:8px;
}

.year-filter select{
padding:6px 10px;
border-radius:8px;
border:1px solid #e2e8f0;
}
</style>