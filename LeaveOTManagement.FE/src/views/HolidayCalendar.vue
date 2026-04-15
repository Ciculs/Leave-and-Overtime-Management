<script setup>
import { ref, computed } from "vue"

/* RECEIVE HOLIDAYS FROM PARENT */
const props = defineProps({
  holidays: {
    type: Array,
    default: () => []
  }
})

/* EMIT CLICKED DATE TO PARENT */
const emit = defineEmits(["date-click"])

/* CURRENT DATE STATE */
const currentDate = ref(new Date())

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const today = new Date()

/* CALENDAR DATA */

const daysInMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
})

const firstDayOfMonth = computed(() => {
  let day = new Date(currentYear.value, currentMonth.value, 1).getDay()
  return day === 0 ? 6 : day - 1
})

/* BUILD HOLIDAY MAP */

const holidayMap = computed(() => {
  const map = {}

  props.holidays.forEach((h) => {
    map[h.holidayDate] = h
  })

  return map
})

/* HELPERS */

const isWeekend = (day) => {
  const d = new Date(currentYear.value, currentMonth.value, day).getDay()
  return d === 0 || d === 6
}

const isToday = (day) => {
  return (
    day === today.getDate() &&
    currentMonth.value === today.getMonth() &&
    currentYear.value === today.getFullYear()
  )
}

const getDateKey = (day) => {
  return `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`
}

const handleDateClick = (day) => {
  emit("date-click", getDateKey(day))
}

/* MONTH NAVIGATION */

const changeMonth = (offset) => {
  currentDate.value = new Date(
    currentYear.value,
    currentMonth.value + offset,
    1
  )
}

const monthLabel = computed(() => {
  const date = currentDate.value

  if (!date || isNaN(date)) return ""

  return date.toLocaleString("default", {
    month: "long"
  })
})
</script>

<template>
  <div class="calendar">
    <div class="calendar-header">
      <button @click="changeMonth(-1)">◀</button>

      <h3>
        {{ monthLabel }} {{ currentYear }}
      </h3>

      <button @click="changeMonth(1)">▶</button>
    </div>

    <Transition name="calendar-slide" mode="out-in">
      <div class="calendar-grid" :key="`${currentYear}-${currentMonth}`">
        <div class="day-name" v-for="d in ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']" :key="d">
          {{ d }}
        </div>

        <div v-for="n in firstDayOfMonth" :key="'empty-' + n" class="empty-cell"></div>

        <div v-for="day in daysInMonth" :key="day" class="day-cell" :class="{
          holiday: holidayMap[getDateKey(day)],
          weekend: isWeekend(day),
          today: isToday(day)
        }" @click="handleDateClick(day)">
          <div class="date-number">
            {{ day }}
          </div>

          <div v-if="holidayMap[getDateKey(day)]" class="holiday-name">
            🎉 {{ holidayMap[getDateKey(day)].name }}
          </div>

          <div v-else class="add-hint">
            + Add holiday
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.calendar {
  margin-top: 20px;
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
  gap: 8px;
}

.day-name {
  font-weight: bold;
  text-align: center;
  color: #707eae;
}

.day-cell {
  background: #f9fbff;
  border-radius: 12px;
  padding: 8px;
  min-height: 100px;
  transition: 0.2s;
  position: relative;
  cursor: pointer;
  border: 1px solid transparent;
}

.day-cell:hover {
  transform: translateY(-2px);
  background: #edf2ff;
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.08);
  border-color: #c7d2fe;
}

.date-number {
  font-weight: bold;
  color: #2b3674;
}

.holiday {
  background: #ffecec;
  border: 1px solid #ffb3b3;
}

.holiday-name {
  font-size: 11px;
  margin-top: 6px;
  background: #ffe0e0;
  padding: 4px 8px;
  border-radius: 8px;
  display: inline-block;
  color: #b91c1c;
  line-height: 1.4;
}

.add-hint {
  margin-top: 8px;
  font-size: 11px;
  color: #94a3b8;
}

.weekend {
  background: #d8e2ff;
}

.today {
  border: 2px solid #6366f1;
}

.empty-cell {
  min-height: 100px;
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
  .calendar-grid {
    gap: 6px;
  }

  .day-cell {
    min-height: 85px;
    padding: 6px;
  }

  .holiday-name {
    font-size: 10px;
    padding: 3px 6px;
  }

  .add-hint {
    font-size: 10px;
  }
}
</style>