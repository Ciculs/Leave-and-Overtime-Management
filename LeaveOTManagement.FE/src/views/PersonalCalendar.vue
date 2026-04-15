<script setup>
import { ref, computed, onMounted } from "vue"
import { useRouter } from "vue-router"
import { getLeaveList } from "@/services/leaveService"
import { getMyOT } from "@/services/otService"
import api from "@/services/api"

/* =========================
   STATE
========================= */
const currentDate = ref(new Date())

const leaves = ref([])
const ots = ref([])
const holidays = ref([])

const selectedDate = ref(null)
const dayEvents = ref([])

const router = useRouter()

/* =========================
   DATE COMPUTED
========================= */
const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

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

/* =========================
   NORMALIZE HELPERS
========================= */
const normalizeDate = (d) => {
  if (!d) return ""
  return new Date(d).toISOString().split("T")[0]
}

const normalizeStatus = (status) => String(status || "").trim().toLowerCase()

/* =========================
   STATUS FILTER
========================= */
const isVisibleLeaveStatus = (status) => {
  const s = normalizeStatus(status)
  return s === "pending" || s === "pending hr" || s === "approved"
}

const isVisibleOTStatus = (status) => {
  const s = normalizeStatus(status)
  return s === "pending" || s === "pending hr" || s === "approved"
}

/* =========================
   LOAD DATA
========================= */
const loadCalendarData = async () => {
  try {
    const leaveRes = await getLeaveList()
    leaves.value = (leaveRes.data || []).filter(item =>
      isVisibleLeaveStatus(item.status)
    )

    const otRes = await getMyOT()
    ots.value = (otRes.data || []).filter(item =>
      isVisibleOTStatus(item.status)
    )

    const holidayRes = await api.get("/holidays")
    holidays.value = holidayRes.data || []

    console.log("LEAVES:", leaves.value)
    console.log("OTS:", ots.value)
    console.log("HOLIDAYS:", holidays.value)
  } catch (err) {
    console.error("Load calendar data failed:", err)
    window.$toast?.("Failed to load personal calendar data.", "error")
  }
}

onMounted(loadCalendarData)

/* =========================
   UI HELPERS
========================= */
const getDateKey = (day) =>
  `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`

const isWeekend = (day) => {
  const weekDay = new Date(currentYear.value, currentMonth.value, day).getDay()
  return weekDay === 0 || weekDay === 6
}

const isToday = (day) => {
  const t = new Date()
  return (
    day === t.getDate() &&
    currentMonth.value === t.getMonth() &&
    currentYear.value === t.getFullYear()
  )
}

/* =========================
   CORE LOGIC
========================= */
const getDayData = (key) => {
  const leave = leaves.value.find(l =>
    isVisibleLeaveStatus(l.status) &&
    normalizeDate(l.fromDate) <= key &&
    normalizeDate(l.toDate) >= key
  )

  const ot = ots.value.find(o =>
    isVisibleOTStatus(o.status) &&
    o.details?.some(d => normalizeDate(d.workDate).startsWith(key))
  )

  const holiday = holidays.value.find(h =>
    normalizeDate(h.holidayDate || h.date).startsWith(key)
  )

  return { leave, ot, holiday }
}

/* =========================
   DAY MAP
========================= */
const dayMap = computed(() => {
  const map = {}
  for (let i = 1; i <= daysInMonth.value; i++) {
    map[i] = getDayData(getDateKey(i))
  }
  return map
})

/* =========================
   CALCULATE OT HOURS
========================= */
const calculateOT = (ot) => {
  const d = ot?.details?.[0]
  if (!d?.fromTime || !d?.toTime) return "0.00"

  const parse = (t) => {
    const [h, m] = t.split(":").map(Number)
    return h + m / 60
  }

  const from = parse(d.fromTime)
  const to = parse(d.toTime)

  const diff = to < from ? (to + 24) - from : to - from
  return diff.toFixed(2)
}

/* =========================
   OPEN DAY MODAL
========================= */
const openDay = (day) => {
  const key = getDateKey(day)
  selectedDate.value = key

  const data = getDayData(key)
  dayEvents.value = []

  if (data.leave) {
    dayEvents.value.push({
      type: "leave",
      data: data.leave
    })
  }

  if (data.ot) {
    dayEvents.value.push({
      type: "ot",
      data: data.ot
    })
  }

  if (data.holiday) {
    dayEvents.value.push({
      type: "holiday",
      data: data.holiday
    })
  }
}

/* =========================
   CHANGE MONTH
========================= */
const changeMonth = (offset) => {
  currentDate.value = new Date(
    currentYear.value,
    currentMonth.value + offset,
    1
  )
}

/* =========================
   ACTIONS
========================= */
const editLeave = (id) => {
  router.push(`/leave/edit/${id}`)
}

const editOT = (id) => {
  router.push(`/ot/edit/${id}`)
}

const cancelLeave = async (id) => {
  if (!confirm("Cancel this leave request?")) return

  try {
    await api.put(`/Leave/${id}/cancel`)
    window.$toast?.("Leave cancelled", "success")

    selectedDate.value = null
    await loadCalendarData()
  } catch (err) {
    console.error(err)
    window.$toast?.("Cancel failed", "error")
  }
}

const cancelOT = async (id) => {
  if (!confirm("Cancel this OT request?")) return

  try {
    await api.put(`/OT/${id}/cancel`)
    window.$toast?.("OT cancelled", "success")

    selectedDate.value = null
    await loadCalendarData()
  } catch (err) {
    console.error(err)
    window.$toast?.("Cancel failed", "error")
  }
}
</script>

<template>
  <div class="calendar-wrapper">
    <h2 class="calendar-title">Personal Calendar</h2>

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

          <div v-for="n in firstDayOfMonth" :key="'e' + n" class="empty-cell"></div>

          <div v-for="day in daysInMonth" :key="day" class="day-cell" :class="{
            weekend: isWeekend(day),
            today: isToday(day),
            'has-leave': dayMap[day]?.leave,
            'has-ot': dayMap[day]?.ot,
            'has-holiday': dayMap[day]?.holiday
          }" @click="openDay(day)">
            <div class="date-number">{{ day }}</div>

            <div class="dots">
              <span v-if="dayMap[day]?.leave" class="dot leave-dot"></span>
              <span v-if="dayMap[day]?.ot" class="dot ot-dot"></span>
              <span v-if="dayMap[day]?.holiday" class="dot holiday-dot"></span>
            </div>

            <div class="tooltip">
              <div v-if="dayMap[day]?.leave">
                📄 {{ dayMap[day].leave.leaveType }}
              </div>

              <div v-if="dayMap[day]?.ot">
                ⏱ {{ calculateOT(dayMap[day].ot) }}h
              </div>

              <div v-if="dayMap[day]?.holiday">
                🎉 {{ dayMap[day].holiday.name }}
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </div>

    <div v-if="selectedDate" class="modal-overlay" @click.self="selectedDate = null">
      <div class="modal-card">
        <button class="modal-close" @click="selectedDate = null">
          ✕
        </button>

        <h3>{{ selectedDate }}</h3>

        <div class="modal-body">
          <div v-if="dayEvents.length === 0">
            No events
          </div>

          <div v-for="(e, index) in dayEvents" :key="index">
            <div v-if="e.type === 'leave'" class="event leave">
              <p><strong>📄 Type:</strong> {{ e.data.leaveType }}</p>

              <p>
                {{ e.data.fromDate?.split("T")[0] }} →
                {{ e.data.toDate?.split("T")[0] }}
              </p>

              <span :class="['status-badge', e.data.status?.toLowerCase()]">
                {{ e.data.status }}
              </span>

              <div class="reason-box">
                {{ e.data.reason }}
              </div>

              <div class="event-actions">
                <button v-if="e.data.status === 'Pending'" class="edit-btn" @click="editLeave(e.data.id)">
                  ✏️ Edit
                </button>

                <button v-if="e.data.status === 'Pending'" class="cancel-btn" @click="cancelLeave(e.data.id)">
                  ❌ Cancel
                </button>
              </div>
            </div>

            <div v-if="e.type === 'ot'" class="event ot">
              <p><strong>⏱ OT Request</strong></p>

              <p>{{ calculateOT(e.data) }} hours</p>

              <span :class="['status-badge', e.data.status?.toLowerCase()]">
                {{ e.data.status }}
              </span>

              <div class="reason-box">
                {{ e.data.reason }}
              </div>

              <div class="event-actions">
                <button v-if="e.data.status === 'Pending'" class="edit-btn" @click="editOT(e.data.id)">
                  ✏️ Edit
                </button>

                <button v-if="e.data.status === 'Pending'" class="cancel-btn" @click="cancelOT(e.data.id)">
                  ❌ Cancel
                </button>
              </div>
            </div>

            <div v-if="e.type === 'holiday'" class="event holiday">
              🎉 {{ e.data.name }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

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
  font-size: 24px;
  font-weight: 700;
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
  min-height: 95px;
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
  color: #2b3674;
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

.has-ot {
  background: #e8f8ec;
}

.has-holiday {
  background: #ffecec;
  border: 1px solid #ffb3b3;
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

.ot-dot {
  background: #16a34a;
}

.holiday-dot {
  background: #dc2626;
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
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.modal-body::-webkit-scrollbar {
  display: none;
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

.event.ot {
  background: #e8f8ec;
}

.event.holiday {
  background: #ffecec;
}

.reason-box {
  margin-top: 10px;
  padding: 12px;
  background: #f8fafc;
  border-left: 4px solid #4318ff;
  border: 1px solid #eee;
  border-radius: 10px;
  font-size: 14px;
  color: #444;
  line-height: 1.5;
  word-break: break-word;
}

.edit-btn {
  background: #d4dbf4;
  color: #4318ff;
}

.edit-btn:hover {
  background: #9badee;
}

.empty-cell {
  min-height: 95px;
}

.status-badge {
  display: inline-block;
  margin-top: 8px;
  padding: 4px 10px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
}

.status-badge.pending {
  background: #fff5e6;
  color: #d97706;
}

.status-badge.approved {
  background: #e6f9f0;
  color: #1a9b5c;
}

.status-badge.rejected {
  background: #ffe6e6;
  color: #dc2626;
}

.status-badge.cancelled {
  background: #f1f5f9;
  color: #64748b;
}

.event-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 10px;
}

.edit-btn,
.cancel-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  height: 36px;
  padding: 0 14px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: 0.2s;
}

.cancel-btn {
  background: #fee2e2;
  color: #dc2626;
}

.cancel-btn:hover {
  background: #fecaca;
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
    min-height: 75px;
    padding: 6px;
  }

  .modal-card {
    width: 100%;
    height: 85vh;
    padding: 20px;
  }
}
</style>