using Application.Cache;
using Application.DTO;
using Application.Repositories;
using Application.Semaphores;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.EventService
{
    public class EventService : IEventService
    {
        protected readonly IEventsRepository _eventRepository;
        protected readonly ICasheRepository _casheRepository;
        public EventService(IEventsRepository eventRepository, ICasheRepository casheRepository)
        {
            _eventRepository = eventRepository;
            _casheRepository = casheRepository;
        }
        public async Task<EventInfo?> GetEvent(Guid id, CancellationToken token = default)
        {
            var requiredEvent = await _casheRepository.GetEvent(id);
            if(requiredEvent != null)
                return new EventInfo(requiredEvent.Id, requiredEvent.Title, requiredEvent.Description, requiredEvent.StartAt, requiredEvent.EndAt, requiredEvent.Status, requiredEvent.TotalSeats, requiredEvent.AvailableSeats);

            requiredEvent = await _eventRepository.Get(id, token);
            if (requiredEvent != null)
            {
                await _casheRepository.AddEventToCashe(requiredEvent);

                return new EventInfo(requiredEvent.Id, requiredEvent.Title, requiredEvent.Description, requiredEvent.StartAt, requiredEvent.EndAt, requiredEvent.Status, requiredEvent.TotalSeats, requiredEvent.AvailableSeats);
            }
            else
                return null;
        }

        public async Task<EventInfo> PostEvent(CreateEvent createEvent, CancellationToken token = default)
        {
            var newEvent = new Event
                (Guid.NewGuid(),
                createEvent.Title,
                createEvent.Description,
                createEvent.StartAt,
                createEvent.EndAt,
                EventStatus.Existing,
                createEvent.TotalSeats,
                createEvent.TotalSeats);

            newEvent = await _eventRepository.Add(newEvent, token);

            return new EventInfo(newEvent.Id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt, newEvent.Status, newEvent.TotalSeats, newEvent.AvailableSeats);
        }

        public async Task<bool> PutEvent(Guid id, CreateEvent createEvent, CancellationToken token = default)
        {
            var eventSemaphore = AppSemaphores.GetSemaphore(id);
            await eventSemaphore.WaitAsync();
            try
            {

                var result = await _eventRepository.Update(id, createEvent, token);
                await _casheRepository.RemoveEventFromCache(id);
                return result;
            }
            finally
            {
                eventSemaphore.Release();
            }
        }
        public async Task<bool> DeleteEvent(Guid id, CancellationToken token = default)
        {
            var eventSemaphore = AppSemaphores.GetSemaphore(id);
            await eventSemaphore.WaitAsync();
            try
            {
                var result = await _eventRepository.Delete(id, token);
                await _casheRepository.RemoveEventFromCache(id);
                return result;
            }
            finally
            {
                eventSemaphore.Release();
            }
        }
        public async Task<PaginatedResult<EventInfo>> GetEventsWithPagination(int pageNumber = 1, int pageSize = 10, string? title = null, DateTime? from = null, DateTime? to = null, CancellationToken token = default)
        {
            var events = await _eventRepository.GetFilteredEventsWithPagination(pageNumber, pageSize, title, from, to, token);
            var totalCount = await _eventRepository.GetFilteredEventsCount(title, from, to, token);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var eventsInfo = events.Select(e => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.Status, e.TotalSeats, e.AvailableSeats)).ToArray();

            return new PaginatedResult<EventInfo>(eventsInfo, pageNumber, totalPages, eventsInfo.Length, totalCount);
        }
        public async Task<EventInfo[]?> GetTopTenEvents(CancellationToken token = default)
        {
            var events = await _casheRepository.GetTopTenEvents();
            if(events == null)
            {
                events = await _eventRepository.GetTopTenEvents(token);
                await _casheRepository.AddTopTenEventsToCashe(events);
            }

            return events.Select(e => new EventInfo(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.Status, e.TotalSeats, e.AvailableSeats)).ToArray();
        }
    }
}
