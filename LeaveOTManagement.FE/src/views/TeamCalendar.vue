<template>
  <div class="calendar-wrapper">
    <h2 class="calendar-title">Team Calendar</h2>

    <div class="calendar">
      <div class="calendar-header">
        <button @click="changeMonth(-1)">◀</button>
        <h3>{{ monthLabel }} {{ currentYear }}</h3>
        <button @click="changeMonth(1)">▶</button>
      </div>

      <Transition name="calendar-slide" mode="out-in">
        <div class="calendar-grid" :key="`${currentYear}-${currentMonth}`">
          <div v-for="d in ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']" :key="d" class="day-name">
            {{ d }}
          </div>

          <div v-for="n in firstDayOfMonth" :key="'empty-' + n" class="empty-cell"></div>

          <div v-for="day in daysInMonth" :key="day" class="day-cell" :class="{
            today: isToday(day),
            weekend: isWeekend(day),
            'has-leave': leaveMap[getKey(day)]?.length,
            'has-holiday': holidayMap[getKey(day)],
            'has-ot': otMap[getKey(day)]?.length
          }" @click="openDay(day)">
            <div class="date-number">{{ day }}</div>

            <div class="dots" v-if="
              leaveMap[getKey(day)]?.length ||
              holidayMap[getKey(day)] ||
              otMap[getKey(day)]?.length
            ">
              <span v-if="leaveMap[getKey(day)]?.length" class="dot leave-dot"></span>
              <span v-if="holidayMap[getKey(day)]" class="dot holiday-dot"></span>
              <span v-if="otMap[getKey(day)]?.length" class="dot ot-dot"></span>
            </div>

            <div v-if="holidayMap[getKey(day)]" class="mini-event holiday-event">
              🎉 {{ holidayMap[getKey(day)] }}
            </div>

            <div v-for="(leave, i) in (leaveMap[getKey(day)] || []).slice(0, 1)" :key="'leave-' + leave.name + '-' + i"
              class="mini-event leave-event">
              📄 {{ leave.name }}
            </div>

            <div v-for="(ot, i) in (otMap[getKey(day)] || []).slice(0, 1)" :key="'ot-' + ot.name + '-' + i"
              class="mini-event ot-event">
              ⏱ {{ ot.name }}
            </div>

            <div v-if="totalEvents(getKey(day)) > 2" class="more-event">
              +{{ totalEvents(getKey(day)) - 2 }} more
            </div>

            <div class="tooltip" v-if="
              leaveMap[getKey(day)]?.length ||
              holidayMap[getKey(day)] ||
              otMap[getKey(day)]?.length
            ">
              <div v-if="holidayMap[getKey(day)]">
                🎉 {{ holidayMap[getKey(day)] }}
              </div>

              <div v-for="(leave, i) in (leaveMap[getKey(day)] || []).slice(0, 3)" :key="'tip-leave-' + i">
                📄 {{ leave.name }} - {{ leave.type }}
              </div>

              <div v-for="(ot, i) in (otMap[getKey(day)] || []).slice(0, 3)" :key="'tip-ot-' + i">
                ⏱ {{ ot.name }} - {{ ot.hours }}
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </div>

    <div v-if="selectedDate" class="modal-overlay" @click.self="selectedDate = null">
      <div class="modal-card">
        <button class="modal-close" @click="selectedDate = null">✕</button>

        <h3>{{ selectedDate }}</h3>

        <div class="modal-body">
          <div v-if="
            selectedLeaves.length === 0 &&
            selectedOTs.length === 0 &&
            !selectedHoliday
          " class="empty-text">
            No events
          </div>

          <div v-if="selectedHoliday" class="event holiday">
            <p><strong>Holiday:</strong> {{ selectedHoliday }}</p>
          </div>

          <div v-for="(leave, index) in selectedLeaves" :key="'modal-leave-' + leave.name + '-' + index"
            class="event leave">
            <p><strong>Employee:</strong> {{ leave.name }}</p>
            <p><strong>Leave Type:</strong> {{ leave.type }}</p>
          </div>

          <div v-for="(ot, index) in selectedOTs" :key="'modal-ot-' + ot.name + '-' + index" class="event ot">
            <p><strong>Employee:</strong> {{ ot.name }}</p>
            <p><strong>OT:</strong> {{ ot.hours }}</p>
            <p><strong>Reason:</strong> {{ ot.reason }}</p>
            <p><strong>Status:</strong> {{ ot.status }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue"
import { getTeamCalendar } from "@/services/leaveService"
import { getHolidayByYear } from "@/services/holidayService"
import { getTeamOT } from "@/services/otService"

const currentDate = ref(new Date())

const leaves = ref([])
const holidays = ref([])
const ots = ref([])

const loadedLeaveMonths = ref([])
const loadedOTMonths = ref([])

const selectedDate = ref(null)
const selectedLeaves = ref([])
const selectedOTs = ref([])
const selectedHoliday = ref("")

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const today = new Date().toISOString().split("T")[0]

const daysInMonth = computed(() =>
  new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
)

const firstDayOfMonth = computed(() => {
  const d = new Date(currentYear.value, currentMonth.value, 1).getDay()
  return d === 0 ? 6 : d - 1
})

const monthLabel = computed(() =>
  currentDate.value.toLocaleString("default", { month: "long" })
)

const normalizeDate = (date) => {
  if (!date) return ""
  return new Date(date).toISOString().split("T")[0]
}

const loadLeaves = async (year, month) => {
  const key = `${year}-${month}`
  if (loadedLeaveMonths.value.includes(key)) return

  try {
    const res = await getTeamCalendar(year, month)
    leaves.value = [...leaves.value, ...(res.data || [])]
    loadedLeaveMonths.value.push(key)
  } catch (err) {
    console.error("Load team leave error", err)
  }
}

const loadOTs = async (year, month) => {
  const key = `${year}-${month}`
  if (loadedOTMonths.value.includes(key)) return

  try {
    const res = await getTeamOT(year, month)
    ots.value = [...ots.value, ...(res.data || [])]
    loadedOTMonths.value.push(key)
  } catch (err) {
    console.error("Load team OT error", err)
  }
}

const loadHolidays = async (year) => {
  try {
    const res = await getHolidayByYear(year)
    holidays.value = res.data || []
  } catch (err) {
    console.error("Load holidays error", err)
  }
}

onMounted(async () => {
  await Promise.all([
    loadLeaves(currentYear.value, currentMonth.value + 1),
    loadOTs(currentYear.value, currentMonth.value + 1),
    loadHolidays(currentYear.value)
  ])
})

watch([currentYear, currentMonth], () => {
  loadLeaves(currentYear.value, currentMonth.value + 1)
  loadOTs(currentYear.value, currentMonth.value + 1)
})

watch(currentYear, async (newYear, oldYear) => {
  if (newYear !== oldYear) {
    leaves.value = []
    ots.value = []
    holidays.value = []
    loadedLeaveMonths.value = []
    loadedOTMonths.value = []

    await Promise.all([
      loadLeaves(newYear, currentMonth.value + 1),
      loadOTs(newYear, currentMonth.value + 1),
      loadHolidays(newYear)
    ])
  }
})

const leaveMap = computed(() => {
  const map = {}

  leaves.value.forEach((l) => {
    const start = new Date(l.startDate)
    const end = new Date(l.endDate)

    for (const d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
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

const holidayMap = computed(() => {
  const map = {}

  holidays.value.forEach((h) => {
    const key = normalizeDate(h.holidayDate || h.date)
    if (key) map[key] = h.name
  })

  return map
})

const parseHours = (fromTime, toTime) => {
  if (!fromTime || !toTime) return "0.00h"

  const parse = (t) => {
    const [h, m] = t.split(":").map(Number)
    return h + m / 60
  }

  const from = parse(fromTime)
  const to = parse(toTime)
  const diff = to < from ? to + 24 - from : to - from

  return `${diff.toFixed(2)}h`
}

const otMap = computed(() => {
  const map = {}

  ots.value.forEach((ot) => {
    const key = normalizeDate(ot.workDate)

    if (!key) return
    if (!map[key]) map[key] = []

    map[key].push({
      id: ot.id,
      name: ot.employeeName,
      reason: ot.reason,
      status: ot.status,
      hours: parseHours(ot.fromTime, ot.toTime)
    })
  })

  return map
})

const getKey = (day) =>
  `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`

const isToday = (day) => getKey(day) === today

const isWeekend = (day) => {
  const d = new Date(currentYear.value, currentMonth.value, day).getDay()
  return d === 0 || d === 6
}

const totalEvents = (key) => {
  let total = 0
  if (holidayMap.value[key]) total += 1
  total += (leaveMap.value[key] || []).length
  total += (otMap.value[key] || []).length
  return total
}

const openDay = (day) => {
  const key = getKey(day)
  selectedDate.value = key
  selectedLeaves.value = leaveMap.value[key] || []
  selectedOTs.value = otMap.value[key] || []
  selectedHoliday.value = holidayMap.value[key] || ""
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
  min-height: 120px;
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

.has-holiday {
  background: #fff1f2;
  border: 1px solid #fecdd3;
}

.has-ot {
  box-shadow: inset 0 0 0 1px #86efac;
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

.holiday-dot {
  background: #dc2626;
}

.ot-dot {
  background: #16a34a;
}

.mini-event {
  margin-top: 4px;
  font-size: 11px;
  padding: 3px 6px;
  border-radius: 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.leave-event {
  background: #eef2ff;
  color: #2b3674;
}

.holiday-event {
  background: #ffe4e6;
  color: #be123c;
  font-weight: 600;
}

.ot-event {
  background: #dcfce7;
  color: #166534;
  font-weight: 600;
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
  min-height: 120px;
}

/* ================= MODAL ================= */

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
  overflow-x: hidden;
  margin-top: 10px;
  padding-right: 5px;
}

/* ================= EVENT ================= */

.event {
  margin-top: 12px;
  padding: 14px;
  border-radius: 12px;
  font-size: 14px;
  overflow: hidden;
}

.event.leave {
  background: #e7f0ff;
}

.event.holiday {
  background: #ffe4e6;
  color: #9f1239;
}

.event.ot {
  background: #dcfce7;
  color: #166534;
}

/* ================= REASON FIX ================= */

.reason-box {
  margin-top: 10px;
  padding: 12px;
  background: #f8fafc;
  border: 1px solid #e5e7eb;
  border-left: 4px solid #4318ff;
  border-radius: 10px;

  font-size: 14px;
  color: #374151;
  line-height: 1.5;

  white-space: pre-wrap;
  word-break: break-word;
  overflow-wrap: anywhere;

  max-height: 140px;
  overflow-y: auto;
  overflow-x: hidden;
}

/* fallback nếu quên bọc reason-box */
.event p,
.event div,
.event span,
.event li {
  word-break: break-word;
  overflow-wrap: anywhere;
}

/* ================= SCROLLBAR ================= */

.reason-box::-webkit-scrollbar,
.modal-body::-webkit-scrollbar {
  width: 6px;
}

.reason-box::-webkit-scrollbar-thumb,
.modal-body::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 999px;
}

/* ================= ANIMATION ================= */

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

/* ================= RESPONSIVE ================= */

@media (max-width: 768px) {
  .calendar-wrapper {
    padding: 15px;
  }

  .calendar-grid {
    gap: 6px;
  }

  .day-cell {
    min-height: 90px;
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