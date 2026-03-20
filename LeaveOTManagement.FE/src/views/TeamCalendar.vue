<template>
  <div class="calendar-wrapper">
    <h2 class="calendar-title">Team Calendar</h2>

    <div class="calendar">
      <!-- HEADER -->
      <div class="calendar-header">
        <button @click="changeMonth(-1)">◀</button>
        <h3>{{ monthLabel }} {{ currentYear }}</h3>
        <button @click="changeMonth(1)">▶</button>
      </div>

      <!-- GRID -->
      <Transition name="calendar-slide" mode="out-in">
        <div class="calendar-grid" :key="`${currentYear}-${currentMonth}`">
          <!-- WEEK HEADER -->
          <div
            v-for="d in ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']"
            :key="d"
            class="day-name"
          >
            {{ d }}
          </div>

          <!-- EMPTY -->
          <div
            v-for="n in firstDayOfMonth"
            :key="'empty-' + n"
            class="empty-cell"
          ></div>

          <!-- DAY -->
          <div
            v-for="day in daysInMonth"
            :key="day"
            class="day-cell"
            :class="{
              today: isToday(day),
              weekend: isWeekend(day),
              'has-leave': leaveMap[getKey(day)]?.length
            }"
            @click="openDay(day)"
          >
            <div class="date-number">{{ day }}</div>

            <!-- DOT -->
            <div class="dots" v-if="leaveMap[getKey(day)]?.length">
              <span class="dot leave-dot"></span>
            </div>

            <!-- PREVIEW -->
            <div
              v-for="(leave, i) in (leaveMap[getKey(day)] || []).slice(0, 2)"
              :key="leave.name + '-' + i"
              class="mini-event"
            >
              {{ leave.name }}
            </div>

            <div
              v-if="(leaveMap[getKey(day)] || []).length > 2"
              class="more-event"
            >
              +{{ leaveMap[getKey(day)].length - 2 }} more
            </div>

            <!-- TOOLTIP -->
            <div class="tooltip" v-if="leaveMap[getKey(day)]?.length">
              <div
                v-for="(leave, i) in leaveMap[getKey(day)].slice(0, 3)"
                :key="'tip-' + i"
              >
                📄 {{ leave.name }} - {{ leave.type }}
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </div>

    <!-- MODAL -->
    <div v-if="selectedDate" class="modal-overlay" @click.self="selectedDate = null">
      <div class="modal-card">
        <button class="modal-close" @click="selectedDate = null">✕</button>

        <h3>{{ selectedDate }}</h3>

        <div class="modal-body">
          <div v-if="selectedLeaves.length === 0">No leave events</div>

          <div
            v-for="(leave, index) in selectedLeaves"
            :key="leave.name + '-' + index"
            class="event leave"
          >
            <p><strong>Employee:</strong> {{ leave.name }}</p>
            <p><strong>Leave Type:</strong> {{ leave.type }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue"
import { getTeamCalendar } from "@/services/leaveService"

const currentDate = ref(new Date())
const leaves = ref([])
const loadedMonths = ref([])

const selectedDate = ref(null)
const selectedLeaves = ref([])

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const today = new Date().toISOString().split("T")[0]

const daysInMonth = computed(() =>
  new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
)

const firstDayOfMonth = computed(() => {
  let d = new Date(currentYear.value, currentMonth.value, 1).getDay()
  return d === 0 ? 6 : d - 1
})

const monthLabel = computed(() =>
  currentDate.value.toLocaleString("default", { month: "long" })
)

const loadLeaves = async (year, month) => {
  const key = `${year}-${month}`
  if (loadedMonths.value.includes(key)) return

  try {
    const res = await getTeamCalendar(year, month)
    leaves.value = [...leaves.value, ...(res.data || [])]
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

const leaveMap = computed(() => {
  const map = {}

  leaves.value.forEach((l) => {
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

const getKey = (day) => {
  return `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`
}

const isToday = (day) => {
  return getKey(day) === today
}

const isWeekend = (day) => {
  const d = new Date(currentYear.value, currentMonth.value, day).getDay()
  return d === 0 || d === 6
}

const openDay = (day) => {
  const key = getKey(day)
  selectedDate.value = key
  selectedLeaves.value = leaveMap.value[key] || []
}

const changeMonth = (offset) => {
  currentDate.value = new Date(
    currentYear.value,
    currentMonth.value + offset,
    1
  )
}
</script>

<style scoped>
.calendar-wrapper {
  background: white;
  padding: 25px;
  border-radius: 16px;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
}

.calendar-title {
  margin-bottom: 18px;
  color: #2b3674;
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
  transition: 0.2s;
}

.calendar-header button:hover {
  background: #dbe3ff;
}

.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 10px;
}

.day-name {
  text-align: center;
  font-weight: 600;
  color: #707eae;
  font-size: 13px;
}

.day-cell {
  background: #f9fbff;
  border-radius: 14px;
  padding: 10px;
  min-height: 110px;
  position: relative;
  cursor: pointer;
  transition: 0.25s;
  overflow: hidden;
}

.day-cell:hover {
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 12px 20px rgba(0, 0, 0, 0.12);
}

.date-number {
  font-weight: 600;
  font-size: 14px;
  margin-bottom: 6px;
}

.weekend {
  background: #d8e2ff;
}

.today {
  border: 2px solid #6366f1;
}

.has-leave {
  background: #e7f0ff;
}

.dots {
  position: absolute;
  bottom: 6px;
  left: 6px;
  display: flex;
  gap: 4px;
}

.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}

.leave-dot {
  background: #2563eb;
}

.mini-event {
  margin-top: 4px;
  font-size: 11px;
  padding: 3px 6px;
  border-radius: 6px;
  background: #eef2ff;
  color: #2b3674;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.more-event {
  margin-top: 4px;
  font-size: 11px;
  color: #6b7280;
}

.tooltip {
  position: absolute;
  bottom: 115%;
  left: 0;
  background: #1e293b;
  color: white;
  padding: 6px 10px;
  border-radius: 6px;
  font-size: 12px;
  opacity: 0;
  pointer-events: none;
  transition: 0.2s;
  white-space: nowrap;
  z-index: 10;
}

.day-cell:hover .tooltip {
  opacity: 1;
}

.empty-cell {
  min-height: 110px;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 1000;
}

.modal-card {
  background: white;
  padding: 25px;
  border-radius: 16px;
  width: 700px;
  max-width: 90vw;
  height: 80vh;
  display: flex;
  flex-direction: column;
  position: relative;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
  animation: modalFade 0.25s ease;
}

.modal-close {
  position: absolute;
  top: 12px;
  right: 14px;
  border: none;
  background: transparent;
  font-size: 18px;
  cursor: pointer;
  color: #666;
  transition: 0.2s;
}

.modal-close:hover {
  color: #000;
  transform: scale(1.2);
}

.modal-body {
  flex: 1;
  overflow-y: auto;
  margin-top: 10px;
  padding-right: 5px;
}

.event {
  margin-top: 12px;
  padding: 14px;
  border-radius: 12px;
  font-size: 14px;
}

.event.leave {
  background: #e7f0ff;
}

@keyframes modalFade {
  from {
    opacity: 0;
    transform: scale(0.9) translateY(10px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

.calendar-slide-enter-active,
.calendar-slide-leave-active {
  transition: all 0.25s ease;
}

.calendar-slide-enter-from {
  opacity: 0;
  transform: translateX(20px);
}

.calendar-slide-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

@media (max-width: 768px) {
  .calendar-wrapper {
    padding: 15px;
  }

  .calendar-grid {
    gap: 6px;
  }

  .day-cell {
    min-height: 85px;
    padding: 6px;
  }

  .mini-event {
    font-size: 10px;
  }

  .modal-card {
    width: 100%;
    height: 85vh;
    padding: 20px;
  }
}
</style>